using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using AutoMapper;
using AutoMapper.Execution;
using Domains;
using InfrastructureLayer.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ApplicationLayer.Services
{
    public class MovieService : BaseService<Movie, MovieDto>, IMovieService
    {
        private readonly IGenericRepository<Movie> _repo;
        private readonly IGenericRepository<TVShow> _showRepo;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITmdbService _tmdbService;



        public MovieService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserService userService,
            IGenericRepository<TVShow> showRepo,
            IGenericRepository<Movie> repo,
            ITmdbService tmdbService


        ) : base(unitOfWork, mapper, userService)
        {
            _repo = repo;
            _showRepo = showRepo;
            _unitOfWork = unitOfWork;
            _repo = _unitOfWork.Repository<Movie>();
            _mapper = mapper;
            _userService = userService;
            _tmdbService = tmdbService;

        }



        public async Task ImportCastForMovieAsync(MovieDto movieDto, int tmdbMovieId)
        {
            var movie = await _unitOfWork.Repository<Movie>()
                .GetFirstOrDefault(x => x.TmdbId == tmdbMovieId);

            if (movie == null)
                throw new Exception($"Movie with TMDB ID {tmdbMovieId} not found.");

            var credits = await _tmdbService.GetMovieCreditsAsync(tmdbMovieId);
            if (credits?.cast == null || !credits.cast.Any())
                return;

            foreach (var c in credits.cast)
            {
                // Check if cast member exists
                var castMember = await _unitOfWork.Repository<CastMember>()
                    .GetFirstOrDefault(x => x.TmdbId == c.id);

                if (castMember == null)
                {
                    castMember = new CastMember
                    {
                        Id = Guid.NewGuid(),
                        Name = c.name,
                        PhotoUrl = c.profile_path != null
                                    ? $"https://image.tmdb.org/t/p/w500{c.profile_path}"
                                    : null,
                        Bio = c.name,
                        TmdbId = c.id,
                        CurrentState = 1
                    };

                    await _unitOfWork.Repository<CastMember>().Add(castMember);
                }

            
                if (!await _unitOfWork.Repository<MovieCast>()
    .AnyAsync(x => x.MovieId == movie.Id && x.CastMemberId == castMember.Id))
                {
                    await _unitOfWork.Repository<MovieCast>().AddAsync(new MovieCast
                    {
                        MovieId = movie.Id,
                        CastMemberId = castMember.Id,
                        CharacterName = c.character
                    });
                }

            }

            await _unitOfWork.SaveChangesAsync();


        }


        public async Task ImportCastForShowsAsync(TvShowDto movieDto, int tmdbMovieId)
        {
            // Get movie from database using the TMDB Id
            var movie = (await _unitOfWork.Repository<TVShow>().GetFirstOrDefault(x => x.TmdbId == tmdbMovieId));

            if (movie == null)
                throw new Exception($"Movie with TMDB ID {tmdbMovieId} not found.");

            var credits = await _tmdbService.GetTvCreditsAsync(tmdbMovieId);
            if (credits?.cast == null || !credits.cast.Any())
                return;

            foreach (var c in credits.cast)
            {
                // Check if cast member exists
                var castMember = (await _unitOfWork.Repository<CastMember>().GetFirstOrDefault(x => x.TmdbId == c.id))
                                    ;

                if (castMember == null)
                {
                    castMember = new CastMember
                    {
                        Id = Guid.NewGuid(),
                        Name = c.name,
                        PhotoUrl = c.profile_path != null
                                    ? $"https://image.tmdb.org/t/p/w500{c.profile_path}"
                                    : null,
                        Bio = c.name,
                        TmdbId = c.id,
                        CurrentState = 1,
                        //CreatedBy = _userService.GetLoggedInUser()

                    };

                    await _unitOfWork.Repository<CastMember>().Add(castMember);
                }

                // Check if already linked
                var exists = movie.Castings.Any(x => x.CastMemberId == castMember.Id);
                if (!exists)
                {
                    movie.Castings.Add(new TvShowCast
                    {

                        TvShowId = movie.Id,
                        CastMemberId = castMember.Id,
                        CharacterName = c.character
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }


        public async Task ImportAllTVsAsync()
        {
            var tvResponse = await _tmdbService.GetAllTvShowsAsync();
            if (tvResponse?.results == null) return;

            foreach (var show in tvResponse.results)
            {
                // Check if TV already exists
                var exists = (await _unitOfWork.Repository<TVShow>().GetAll())
                                .Any(x => x.TmdbId == show.id);

                if (exists) continue;

                // Get trailer for show
                var trailerData = await _tmdbService.GetTvVideosAsync(show.id);
                var showTrailerKey = trailerData?.results?
                    .FirstOrDefault(v => v.site == "YouTube" && v.type == "Trailer")?.key;

                // Create TV Show Entity
                var tvEntity = new TVShow
                {
                    Id = Guid.NewGuid(),
                    Title = show.name,
                    Name = show.original_name,
                    TmdbId = show.id,
                    Description = show.overview,
                    PosterUrl = show.poster_path != null ? $"https://image.tmdb.org/t/p/w500{show.poster_path}" : "",
                    Language = show.original_language,
                    AgeRating = AgeRating.AllAges,
                    AudioType = "Dolby Digital",
                    IsFeatured = false,
                    Type = MediaType.TvShow,
                    ReleaseYear = !string.IsNullOrEmpty(show.first_air_date) ?
                                   int.Parse(show.first_air_date.Split("-")[0]) : null
                };

                // Assign Genres
                if (show.genre_ids != null)
                {
                    foreach (var gId in show.genre_ids)
                    {
                        var genre = (await _unitOfWork.Repository<Genre>().GetAll())
                                        .FirstOrDefault(x => x.TmdbId == gId);

                        if (genre != null)
                        {
                            tvEntity.TVShowGenres.Add(new TVShowGenre
                            {
                                TVShowId = tvEntity.Id,
                                GenreId = genre.Id
                            });
                        }
                    }
                }

                // Save TV Show first (so children FK attach)
                await _unitOfWork.Repository<TVShow>().Add(tvEntity);
                await _unitOfWork.SaveChangesAsync();


                // ---------------- Seasons & Episodes ----------------
                var showDetails = await _tmdbService.GetTvDetailsAsync(show.id);

                if (showDetails?.seasons != null)
                {
                    foreach (var seasonInfo in showDetails.seasons.Where(s => s.season_number > 0))
                    {
                        var season = new Season
                        {
                            Id = Guid.NewGuid(),
                            TvShowId = tvEntity.Id,
                            SeasonNumber = seasonInfo.season_number
                        };

                        await _unitOfWork.Repository<Season>().Add(season);
                        await _unitOfWork.SaveChangesAsync();

                        // Fetch episodes
                        var seasonDetails = await _tmdbService.GetSeasonDetailsAsync(show.id, seasonInfo.season_number);

                        if (seasonDetails?.episodes != null)
                        {
                            foreach (var ep in seasonDetails.episodes)
                            {
                                // Try get trailer for specific episode (optional)
                                var epVideos = await _tmdbService.GetEpisodeVideosAsync(show.id, seasonInfo.season_number, ep.episode_number);
                                var episodeTrailerKey = epVideos?.results?
                                    .FirstOrDefault(x => x.site == "YouTube" && x.type == "Trailer")?.key;

                                var episodeEntity = new Episode
                                {
                                    Id = Guid.NewGuid(),
                                    SeasonId = season.Id,
                                    Title = ep.name ?? $"Episode {ep.episode_number}",
                                    EpisodeNumber = ep.episode_number,
                                    DurationMinutes = ep.runtime ?? 0,

                                    // Use trailer if exists; otherwise fallback to show trailer
                                    TrailerUrl = episodeTrailerKey != null
                                                    ? $"https://www.youtube.com/watch?v={episodeTrailerKey}"
                                                    : (showTrailerKey != null ? $"https://www.youtube.com/watch?v={showTrailerKey}" : ""),

                                    // Video stored later when streaming server exists
                                    VideoUrl = ""
                                };

                                await _unitOfWork.Repository<Episode>().Add(episodeEntity);
                            }

                            await _unitOfWork.SaveChangesAsync();
                        }
                    }
                }

                // ------------------- CAST -------------------
                var castData = await _tmdbService.GetTvCastAsync(tvEntity.TmdbId);

                if (castData?.cast != null)
                {
                    foreach (var member in castData.cast)
                    {
                        // Check if CastMember exists
                        var existingCast = (await _unitOfWork.Repository<CastMember>().GetAll())
                                                .FirstOrDefault(c => c.TmdbId == member.id);

                        if (existingCast == null)
                        {
                            existingCast = new CastMember
                            {
                                Id = Guid.NewGuid(),
                                Name = member.name,
                                PhotoUrl = member.profile_path != null ?
                                            $"https://image.tmdb.org/t/p/w500{member.profile_path}" : "",
                                TmdbId = member.id
                            };

                            // Add the CastMember first
                            await _unitOfWork.Repository<CastMember>().Add(existingCast);
                            await _unitOfWork.SaveChangesAsync(); // حفظ عشان نقدر نربط FK بعد كده
                        }

                        // Link CastMember to TVShow
                        var tvCast = new TvShowCast
                        {
                            Id = Guid.NewGuid(), // لو انت ضيفت BaseTable لـ TvShowCast
                            TvShowId = tvEntity.Id,
                            CastMemberId = existingCast.Id,
                            CharacterName = member.character ?? ""
                        };

                        await _unitOfWork.Repository<TvShowCast>().Add(tvCast);
                    }

                    await _unitOfWork.SaveChangesAsync();
                }

            }
        }


        public async Task ImportTopRatedShowsAsync()
        {
            var showsResponse = await _tmdbService.GetTopRatedShowsAsync();
            if (showsResponse?.results == null) return;

            foreach (var show in showsResponse.results)
            {
                // Check if TV already exists
                var exists = (await _unitOfWork.Repository<TVShow>().GetAll())
                                .Any(x => x.TmdbId == show.id);
                if (exists) continue;

                // Get show trailer
                var trailerData = await _tmdbService.GetTvVideosAsync(show.id);
                var showTrailerKey = trailerData?.results?
                    .FirstOrDefault(v => v.site == "YouTube" && v.type == "Trailer")?.key;

                // Create TV Show entity
                var tvEntity = new TVShow
                {
                    Id = Guid.NewGuid(),
                    Title = show.original_name,
                    Name = show.name,
                    TmdbId = show.id,
                    Description = show.overview,
                    PosterUrl = show.poster_path != null ? $"https://image.tmdb.org/t/p/w500{show.poster_path}" : "",
                    Language = show.original_language,
                    AgeRating = AgeRating.AllAges,
                    AudioType = "Dolby Digital",
                    Type = MediaType.TvShow,
                    IsFeatured = true,
                    ReleaseYear = !string.IsNullOrEmpty(show.first_air_date) ?
                                   int.Parse(show.first_air_date.Split("-")[0]) : null
                };

                // Assign Genres
                if (show.genre_ids != null)
                {
                    foreach (var gId in show.genre_ids)
                    {
                        var genre = (await _unitOfWork.Repository<Genre>().GetAll())
                                        .FirstOrDefault(x => x.TmdbId == gId);

                        if (genre != null)
                        {
                            tvEntity.TVShowGenres.Add(new TVShowGenre
                            {
                                TVShowId = tvEntity.Id,
                                GenreId = genre.Id
                            });
                        }
                    }
                }

                // Save TV Show first
                await _unitOfWork.Repository<TVShow>().Add(tvEntity);
                await _unitOfWork.SaveChangesAsync();

                // ---------------- Seasons & Episodes ----------------
                var showDetails = await _tmdbService.GetTvDetailsAsync(show.id);

                if (showDetails?.seasons != null)
                {
                    foreach (var seasonInfo in showDetails.seasons.Where(s => s.season_number > 0))
                    {
                        var season = new Season
                        {
                            Id = Guid.NewGuid(),
                            TvShowId = tvEntity.Id,
                            SeasonNumber = seasonInfo.season_number
                        };

                        await _unitOfWork.Repository<Season>().Add(season);
                        await _unitOfWork.SaveChangesAsync();

                        // Fetch episodes
                        var seasonDetails = await _tmdbService.GetSeasonDetailsAsync(show.id, seasonInfo.season_number);

                        if (seasonDetails?.episodes != null)
                        {
                            foreach (var ep in seasonDetails.episodes)
                            {
                                var epVideos = await _tmdbService.GetEpisodeVideosAsync(show.id, seasonInfo.season_number, ep.episode_number);
                                var episodeTrailerKey = epVideos?.results?
                                    .FirstOrDefault(x => x.site == "YouTube" && x.type == "Trailer")?.key;

                                var episodeEntity = new Episode
                                {
                                    Id = Guid.NewGuid(),
                                    SeasonId = season.Id,
                                    Title = ep.name ?? $"Episode {ep.episode_number}",
                                    EpisodeNumber = ep.episode_number,
                                    DurationMinutes = ep.runtime ?? 0,
                                    TrailerUrl = episodeTrailerKey != null
                                                    ? $"https://www.youtube.com/watch?v={episodeTrailerKey}"
                                                    : (showTrailerKey != null ? $"https://www.youtube.com/watch?v={showTrailerKey}" : ""),
                                    VideoUrl = "" // Optional: fill when streaming video is available
                                };

                                await _unitOfWork.Repository<Episode>().Add(episodeEntity);
                            }

                            await _unitOfWork.SaveChangesAsync();
                        }
                    }
                }

                // ------------------- CAST -------------------
                var tvDto = _mapper.Map<TvShowDto>(tvEntity);
                var castData = await _tmdbService.GetTvCastAsync(tvEntity.TmdbId);

                if (castData?.cast != null)
                {
                    foreach (var member in castData.cast)
                    {
                        // Check if CastMember exists
                        var existingCast = (await _unitOfWork.Repository<CastMember>().GetAll())
                                            .FirstOrDefault(c => c.TmdbId == member.id);

                        if (existingCast == null)
                        {
                            existingCast = new CastMember
                            {
                                Id = Guid.NewGuid(),
                                Name = member.name,
                                PhotoUrl = member.profile_path != null ?
                                              $"https://image.tmdb.org/t/p/w500{member.profile_path}" : "",
                                TmdbId = member.id
                            };

                            await _unitOfWork.Repository<CastMember>().Add(existingCast);
                            await _unitOfWork.SaveChangesAsync();
                        }

                        // Link CastMember to TVShow
                        var tvCast = new TvShowCast
                        {
                            TvShowId = tvEntity.Id,
                            CastMemberId = existingCast.Id,
                            CharacterName = member.character ?? ""
                        };

                        await _unitOfWork.Repository<TvShowCast>().Add(tvCast);
                    }

                    await _unitOfWork.SaveChangesAsync();
                }
            }
        }



        public async Task ImportAllMoviesAsync()
        {
            var moviesResponse = await _tmdbService.GetAllMoviesAsync();
            if (moviesResponse?.results == null) return;

            foreach (var m in moviesResponse.results)
            {
                // Check if exists
                var existingMovie = (await _unitOfWork.Repository<Movie>().GetAll())
                                        .FirstOrDefault(x => x.TmdbId == m.id);
                if (existingMovie != null)
                    continue;

                var trailer = await _tmdbService.GetMovieVideosAsync(m.id);
                var trailerKey = trailer?.results.FirstOrDefault(v => v.type == "Trailer" && v.site == "YouTube")?.key;

                var movie = new Movie
                {
                    Id = Guid.NewGuid(),
                    Title = m.title,
                    TmdbId = m.id,
                    IsFeatured = false,
                    AudioType = "Dolby Digital",
                    AgeRating = AgeRating.AllAges,
                    Type = MediaType.Movie,
                    CurrentState = 1,
                    Language = m.original_language,
                    Description = m.overview,
                    PosterUrl = m.poster_path != null ? $"https://image.tmdb.org/t/p/w500{m.poster_path}" : "",
                    VideoUrl = trailerKey != null ? $"https://www.youtube.com/watch?v={trailerKey}" : "",
                    TrailerUrl = trailerKey != null ? $"https://www.youtube.com/watch?v={trailerKey}" : "",
                    ReleaseYear = int.TryParse(m.release_date?.Split('-')[0], out int y) ? y : 0,
                };

                // Add Genres
                if (m.genre_ids != null)
                {
                    foreach (var gid in m.genre_ids)
                    {
                        var genre = (await _unitOfWork.Repository<Genre>().GetAll())
                                    .FirstOrDefault(x => x.TmdbId == gid);

                        if (genre != null)
                        {
                            movie.MovieGenres.Add(new MovieGenre
                            {
                                MovieId = movie.Id,
                                GenreId = genre.Id
                            });
                        }
                    }
                }

                await _unitOfWork.Repository<Movie>().Add(movie);
                await _unitOfWork.SaveChangesAsync();

                // ------------------- CAST -------------------
                var castData = await _tmdbService.GetMovieCastAsync(movie.TmdbId);

                if (castData?.cast != null)
                {
                    foreach (var member in castData.cast)
                    {
                        // Check if CastMember exists
                        var existingCast = (await _unitOfWork.Repository<CastMember>().GetAll())
                                                .FirstOrDefault(c => c.TmdbId == member.id);

                        if (existingCast == null)
                        {
                            existingCast = new CastMember
                            {
                                Id = Guid.NewGuid(),
                                Name = member.name,
                                PhotoUrl = member.profile_path != null ?
                                            $"https://image.tmdb.org/t/p/w500{member.profile_path}" : "",
                                TmdbId = member.id
                            };

                            // Add the CastMember first
                            await _unitOfWork.Repository<CastMember>().Add(existingCast);
                            await _unitOfWork.SaveChangesAsync(); // حفظ عشان نقدر نربط FK بعد كده
                        }

                        // Link CastMember to Movie
                        var movieCast = new MovieCast
                        {
                            Id = Guid.NewGuid(), // لو MovieCast وارث BaseTable
                            MovieId = movie.Id,
                            CastMemberId = existingCast.Id,
                            CharacterName = member.character ?? ""
                        };

                        await _unitOfWork.Repository<MovieCast>().Add(movieCast);
                    }

                    await _unitOfWork.SaveChangesAsync();
                }
            }
        }


        public async Task ImportTopRatedMoviesAsync()
        {
            var moviesResponse = await _tmdbService.GetTopRatedMoviesAsync();
            if (moviesResponse?.results == null) return;

            foreach (var m in moviesResponse.results)
            {
                // Check if exists
                var existingMovie = (await _unitOfWork.Repository<Movie>().GetAll())
                                    .FirstOrDefault(x => x.TmdbId == m.id);
                if (existingMovie != null)
                    continue;

                var trailer = await _tmdbService.GetMovieVideosAsync(m.id);
                var trailerKey = trailer?.results.FirstOrDefault(v => v.type == "Trailer" && v.site == "YouTube")?.key;

                var movie = new Movie
                {
                    Id = Guid.NewGuid(),
                    Title = m.title,
                    TmdbId = m.id,
                    IsFeatured = true,
                    AudioType = "Dolby Digital",
                    AgeRating = AgeRating.AllAges,
                    Type = MediaType.Movie,
                    //CreatedBy = _userService.GetLoggedInUser(),
                    CurrentState = 1,
                    Language = m.original_language,
                    Description = m.overview,
                    PosterUrl = m.poster_path != null ? $"https://image.tmdb.org/t/p/w500{m.poster_path}" : "",
                    VideoUrl = trailerKey != null ? $"https://www.youtube.com/watch?v={trailerKey}" : "",
                    TrailerUrl = trailerKey != null ? $"https://www.youtube.com/watch?v={trailerKey}" : "",
                    ReleaseYear = int.TryParse(m.release_date?.Split('-')[0], out int y) ? y : 0,
                };

                // Add Genres
                if (m.genre_ids != null)
                {
                    foreach (var gid in m.genre_ids)
                    {
                        var genre = (await _unitOfWork.Repository<Genre>().GetAll())
                                    .FirstOrDefault(x => x.TmdbId == gid);

                        if (genre != null)
                        {
                            movie.MovieGenres.Add(new MovieGenre
                            {
                                MovieId = movie.Id,
                                GenreId = genre.Id
                            });
                        }
                    }
                }

                await _unitOfWork.Repository<Movie>().Add(movie);
                await _unitOfWork.SaveChangesAsync();

                // ------------------- CAST -------------------
                var castData = await _tmdbService.GetMovieCastAsync(movie.TmdbId);

                if (castData?.cast != null)
                {
                    foreach (var member in castData.cast)
                    {
                        // Check if CastMember exists
                        var existingCast = (await _unitOfWork.Repository<CastMember>().GetAll())
                                                .FirstOrDefault(c => c.TmdbId == member.id);

                        if (existingCast == null)
                        {
                            existingCast = new CastMember
                            {
                                Id = Guid.NewGuid(),  
                                Name = member.name,
                                PhotoUrl = member.profile_path != null ?
                                            $"https://image.tmdb.org/t/p/w500{member.profile_path}" : "",
                                TmdbId = member.id
                            };

                            // Add the CastMember first
                            await _unitOfWork.Repository<CastMember>().Add(existingCast);
                            await _unitOfWork.SaveChangesAsync(); // حفظ عشان نقدر نربط FK بعد كده
                        }

                        // Link CastMember to Movie
                        var movieCast = new MovieCast
                        {
                            Id = Guid.NewGuid(), // لو MovieCast وارث BaseTable
                            MovieId = movie.Id,
                            CastMemberId = existingCast.Id,
                            CharacterName = member.character ?? ""
                        };

                        await _unitOfWork.Repository<MovieCast>().Add(movieCast);
                    }

                    await _unitOfWork.SaveChangesAsync();
                }
            }
        }


        public async Task<List<AllMediaDto>> GetRandomMediaAsync(int count)
        {
            // Step 1: Get random movies
            var randomMovies = await _repo.GetAllQueryable()
                .OrderBy(m => Guid.NewGuid())
                .Take(count)
                .ToListAsync();

            // Step 2: Get random TV shows
            var randomShows = await _repo.GetAllQueryable()
                .OrderBy(s => Guid.NewGuid())
                .Take(count)
                .ToListAsync();

            // Step 3: Merge results
            var combined = new List<AllMediaDto>();

            combined.AddRange(_mapper.Map<List<AllMediaDto>>(randomMovies));
            combined.AddRange(_mapper.Map<List<AllMediaDto>>(randomShows));

            // Step 4: Shuffle again and limit to requested count
            var result = combined
                .OrderBy(x => Guid.NewGuid())
                .Take(count)
                .ToList();

            return result;
        }


        // ====================================
        // Get All Movie with Simple Filters
        // ====================================
        public async Task<IEnumerable<MovieDto>> GetAllAsync(Guid? genreId = null, string? search = null)
        {
            var query = await _repo.GetList(m => m.CurrentState == 1);

            if (genreId.HasValue)
                query = query.Where(m => m.MovieGenres.Any(g => g.GenreId == genreId.Value)).ToList();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            return _mapper.Map<IEnumerable<MovieDto>>(query);
        }


        // ===========================
        // Get All with Filters
        // ===========================
        public async Task<IEnumerable<MovieDto>> GetAllByFilter(MovieSearchFilterDto filter)
        {

            var query = await _repo.GetList(m => m.CurrentState == 1);

            // ===== Search =====
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(m =>
                    m.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (m.Description != null && m.Description.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    m.Castings.Any(c => c.CastMember.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    m.MovieGenres.Any(g => g.Genre.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            // ===== Filter by Language =====
            if (!string.IsNullOrWhiteSpace(filter.Language))
            {
                query = query.Where(m =>
                m.Language != null && m.Language.Equals(filter.Language, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // ===== Filter by AudioType =====
            if (!string.IsNullOrWhiteSpace(filter.AudioType))
            {
                query = query.Where(m => m.AudioType != null && m.AudioType.Equals(filter.AudioType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // ===== Sort =====
            if (!string.IsNullOrWhiteSpace(filter.SortOption))
            {
                query = filter.SortOption switch
                {
                    "YearRelease" => query.OrderByDescending(m => m.ReleaseYear).ToList(),
                    "A-Z" => query.OrderBy(m => m.Title).ToList(),
                    "Z-A" => query.OrderByDescending(m => m.Title).ToList(),
                    _ => query
                };
            }

            return _mapper.Map<IEnumerable<MovieDto>>(query);
        }


        // ===========================
        public async Task<MovieDto?> GetByIdAsync(Guid id)
        {
            var movies = await _unitOfWork.Repository<Movie>()
                .GetListWithInclude(
                    filter: x => x.Id == id,
                    include: query => query
                        .Include(x => x.MovieGenres)
                            .ThenInclude(g => g.Genre)
                        .Include(x => x.Castings)
                            .ThenInclude(c => c.CastMember)
                );

            var movie = movies.FirstOrDefault();

            if (movie == null)
                return null;

            // تحديد أول 10 من الكاست
            movie.Castings = movie.Castings
                .Where(c => c.CastMember != null)
                .OrderBy(c => c.Id)
                .Take(10)
                .ToList();

            return _mapper.Map<MovieDto>(movie);
        }


        public async Task<MovieDto?> GetTrailerByIdAsync(Guid id)
        {
            // إضافة شرط للتأكد من أن الفيلم مميز
            var entity = await _repo.GetFirstOrDefault(m => m.Id == id && m.IsFeatured && m.CurrentState == 1);

            // إذا لم يوجد الفيلم أو لم يكن مميزًا، نرجع null
            return entity == null ? null : _mapper.Map<MovieDto>(entity);
        }


        // ===========================
        // Get Movies By Genre (paged)
        // ===========================
        //public async Task<GenreMoviesResponseDto> GetMoviesByGenreAsync(Guid genreId, int page = 1, int pageSize = 20)
        //{
        //    // Load genre name for response metadata
        //    var genreRepo = _unitOfWork.Repository<Genre>();
        //    var genre = await genreRepo.GetById(genreId);

        //    // Query movies filtered by genre on the DB side and include relations needed for mapping
        //    var paged = await _repo.GetPagedList<Movie>(
        //        pageNumber: page,
        //        pageSize: pageSize,
        //        filter: m => m.CurrentState == 1 && m.MovieGenres.Any(mg => mg.GenreId == genreId),
        //        selector: null, // return Movie entities
        //        orderBy: m => m.ReleaseYear,
        //        isDescending: true,
        //        // include genres and cast for mapping enrichment
        //        m => m.MovieGenres,
        //        m => m.Castings
        //    );

        //    var moviesDto = _mapper.Map<List<MovieDto>>(paged.Items);

        //    return new GenreMoviesResponseDto
        //    {
        //        GenreId = genreId,
        //        GenreName = genre?.Name ?? string.Empty,
        //        MediaData = moviesDto,
        //        TotalCount = paged.TotalCount,
        //        Page = page,
        //        PageSize = pageSize
        //    };
        //}

        public async Task<GenreMoviesResponseDto> GetMoviesByGenreAsync(Guid genreId, int page = 1, int pageSize = 20)
        {
            var genreRepo = _unitOfWork.Repository<Genre>();
            var genre = await genreRepo.GetById(genreId);

            var paged = await _repo.GetPagedListAsync(
                pageNumber: page,
                pageSize: pageSize,
                filter: m => m.CurrentState == 1 && m.MovieGenres.Any(mg => mg.GenreId == genreId),
                orderBy: m => m.ReleaseYear,
                isDescending: true,
                include: query => query
                    .Include(m => m.MovieGenres)
                        .ThenInclude(mg => mg.Genre)
                    .Include(m => m.Castings)
                        .ThenInclude(c => c.CastMember)
            );

            var moviesDto = _mapper.Map<List<MovieDto>>(paged.Items);

            return new GenreMoviesResponseDto
            {
                GenreId = genreId,
                GenreName = genre?.Name ?? string.Empty,
                MediaData = moviesDto,
                TotalCount = paged.TotalCount,
                Page = page,
                PageSize = pageSize
            };
        }



        public async Task<GenreMoviesResponseDto> GetMoviesByGenreNameAsync(string genreName, int page = 1, int pageSize = 20)
        {
            var genreRepo = _unitOfWork.Repository<Genre>();
            var genre = await genreRepo.GetFirstOrDefault(g => g.Name.ToLower() == genreName.ToLower());

            if (genre == null)
                return null;

            var paged = await _repo.GetPagedList<Movie>(
                pageNumber: page,
                pageSize: pageSize,
                filter: m => m.CurrentState == 1 && m.MovieGenres.Any(mg => mg.GenreId == genre.Id),
                selector: null,
                orderBy: m => m.ReleaseYear,
                isDescending: true,
                m => m.MovieGenres,
                m => m.Castings
            );

            var moviesDto = _mapper.Map<List<MovieDto>>(paged.Items);

            return new GenreMoviesResponseDto
            {
                GenreId = genre.Id,
                GenreName = genre.Name,
                MediaData = moviesDto,
                TotalCount = paged.TotalCount,
                Page = page,
                PageSize = pageSize
            };
        }


        // ===========================
        // Create
        // ===========================
        public async Task<MovieDto> CreateAsync(MovieDto dto)
        {
            var entity = _mapper.Map<Movie>(dto);

            entity.Id = Guid.NewGuid();
            entity.CreatedDate = DateTime.UtcNow;
            entity.CreatedBy = _userService.GetLoggedInUser();
            entity.CurrentState = 1;

            await _repo.Add(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<MovieDto>(entity);
        }

        // ===========================
        // Update
        // ===========================
        public async Task<bool> UpdateAsync(Guid id, MovieDto dto)
        {
            var entity = await _repo.GetById(id);
            if (entity == null) return false;

            _mapper.Map(dto, entity);

            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = _userService.GetLoggedInUser();

            var result = await _repo.Update(entity);

            if (result)
                await _unitOfWork.SaveChangesAsync();

            return result;
        }

        // ===========================
        // Delete (Soft Delete)
        // ===========================
        public async Task<bool> DeleteAsync(Guid id)
        {
            var result = await _repo.ChangeStatus(id, _userService.GetLoggedInUser(), 0);

            if (result)
                await _unitOfWork.SaveChangesAsync();

            return result;
        }

        // ===========================
        // Featured Movies
        // ===========================
        public async Task<IEnumerable<MovieDto>> GetFeaturedAsync(int limit = 10)
        {
            var movies = await _unitOfWork.Repository<Movie>()
                .GetListWithInclude(
                    filter: m => m.CurrentState == 1 && m.IsFeatured,
                    include: query => query
                        .Include(x => x.MovieGenres)
                            .ThenInclude(mg => mg.Genre)
                        .Include(x => x.Castings)
                            .ThenInclude(c => c.CastMember)
                );

            var ordered = movies
                .OrderByDescending(m => m.CreatedDate)
                .Take(limit)
                .Select(m =>
                {
                    // هنا نعمل Take للـ Castings
                    m.Castings = m.Castings
                        .OrderBy(c => c.Id)
                        .Take(10)
                        .ToList();

                    return m;
                })
                .ToList();

            return _mapper.Map<IEnumerable<MovieDto>>(ordered);
        }




        /// <summary>
        /// that i neeeeeeeeeeeeeeeeeeeeeedd
        /// </summary>
        /// <returns></returns>

        public async Task<IEnumerable<MovieDto>> GetAllMoviesAsync()
        {
            // Load Movies with Related Entities
            var movies = await _unitOfWork.Repository<Movie>()
                .GetListWithInclude(
                    filter: x => true,
                    include: query => query
                        .Include(x => x.Castings)
                            .ThenInclude(c => c.CastMember)  // مهم جداً عشان CastMember مايبقاش null
                        .Include(x => x.MovieGenres)
                            .ThenInclude(mg => mg.Genre)
                );

            // Map to DTO with limiting cast
            var result = movies.Select(m => new MovieDto
            {
                Id = m.Id,
                Title = m.Title,
                Description= m.Description,
                ReleaseYear=m.ReleaseYear,
                DurationMinutes=m.DurationMinutes,
                AgeRating=m.AgeRating,
                PosterUrl=m.PosterUrl,
                TmdbId=m.TmdbId,
                VideoUrl=m.VideoUrl,
                TrailerUrl=m.TrailerUrl,
                IsFeatured=m.IsFeatured,
                Type=m.Type,
                Language=m.Language,
                AudioType=m.AudioType,
                GenresNames = m.MovieGenres.Select(mg => new GenreDto
                {
                    Id = mg.Genre.Id,
                    Name = mg.Genre.Name
                }).ToList(),

                Castings = m.Castings
                    .Where(c => c.CastMember != null)  // حماية من null لو فيه بيانات ناقصة
                    .OrderBy(c => c.Id)
                    .Take(10)
                    .Select(c => new CastDto
                    {
                        Id = c.CastMember.Id,
                        Name = c.CastMember.Name,
                        CharacterName = c.CharacterName
                    }).ToList()

            }).ToList();

            return result;
        }


        public async Task<List<MovieDto>> GetFeaturedWithTrailersAsync(int limit = 10)
        {
            var movies = await _repo.GetList(m => m.IsFeatured && m.CurrentState == 1);

            var featuredMovies = movies.OrderByDescending(m => m.CreatedDate).Take(limit).ToList();

            // باستخدام AutoMapper لتحويل الـ Movie إلى MovieDto
            return _mapper.Map<List<MovieDto>>(featuredMovies);
        }

        // ===========================
        // Streaming URL (placeholder)
        // ===========================
        public async Task<string?> GetStreamingUrlAsync(Guid movieId, Guid profileId)
        {
            var movie = await _repo.GetById(movieId);
            if (movie == null) return null;

            // NOTE:
            // - Real streaming locators should be produced by a dedicated DRM/CDN service.
            // - Here we return TrailerUrl (if present) as a placeholder streaming locator.
            // - You can extend this to verify the profile's subscription before returning the URL.
            return !string.IsNullOrWhiteSpace(movie.TrailerUrl) ? movie.TrailerUrl : movie.PosterUrl;
        }

        // ===========================
        // Get by multiple genre ids
        // ===========================
        public async Task<IEnumerable<MovieDto>> GetByGenreIdsAsync(IEnumerable<Guid> genreIds, int limit = 50)
        {
            var ids = genreIds?.ToList() ?? new List<Guid>();
            if (!ids.Any()) return Enumerable.Empty<MovieDto>();

            var list = await _repo.GetList(m => m.CurrentState == 1 && m.MovieGenres.Any(mg => ids.Contains(mg.GenreId)));
            var ordered = list.OrderByDescending(m => m.ReleaseYear).Take(limit).ToList();
            return _mapper.Map<IEnumerable<MovieDto>>(ordered);
        }













        // ===========================
        // import data from tmdb


        public async Task ImportGenresAsync()
        {
            var genresResponse = await _tmdbService.GetGenresAsync();

            if (genresResponse?.genres == null) return;

            foreach (var g in genresResponse.genres)
            {



                await _unitOfWork.Repository<Genre>().Add(new Genre
                {
                    Id = Guid.NewGuid(),
                    Name = g.name,
                    TmdbId = g.id,
                    CurrentState = 1,
                    //CreatedBy = _userService.GetLoggedInUser()
                });

            }

            await _unitOfWork.SaveChangesAsync();
        }

        //public async Task ImportTopRatedMoviesAsync()
        //{
        //    var moviesResponse = await _tmdbService.GetTopRatedMoviesAsync();
        //    if (moviesResponse?.results == null) return;

        //    foreach (var m in moviesResponse.results)
        //    {
        //        var trailer = await _tmdbService.GetMovieVideosAsync(m.id);
        //        var trailerKey = trailer?.results.FirstOrDefault(v => v.type == "Trailer" && v.site == "YouTube")?.key;

        //        var movie = new Movie
        //        {
        //            Title = m.title,
        //            Description = m.overview,
        //            PosterUrl = m.poster_path != null ? $"https://image.tmdb.org/t/p/w500{m.poster_path}" : "",
        //            VideoUrl = trailerKey != null ? $"https://www.youtube.com/watch?v={trailerKey}" : "",
        //            TrailerUrl = trailerKey != null ? $"https://www.youtube.com/watch?v={trailerKey}" : "",
        //            ReleaseYear = int.TryParse(m.release_date?.Split('-')[0], out int y) ? y : 0,
        //        };

        //        // Genres
        //        if (m.genre_ids != null)
        //        {
        //            foreach (var gid in m.genre_ids)
        //            {
        //                //var genre = _unitOfWork.Repository<Genre>().Query().FirstOrDefault(x => x.TmdbId == gid);
        //                var genre = (await _unitOfWork.Repository<Genre>().GetAll())
        //                        .FirstOrDefault(x => x.TmdbId == gid);
        //                //var genre = _unitOfWork.Repository<Genre>().GetAll().Result.FirstOrDefault(x => x.TmdbId == gid);

        //                if (genre != null)
        //                {
        //                    movie.MovieGenres.Add(new MovieGenre
        //                    {
        //                        Genre = genre,
        //                        GenreId = genre.Id
        //                    });
        //                }
        //            }
        //        }

        //        await _unitOfWork.Repository<Movie>().Add(movie);
        //    }

        //    await _unitOfWork.SaveChangesAsync();
        //}

        //public async Task ImportCastForMovieAsync(Movie movie, int tmdbMovieId)
        //{
        //    var credits = await _tmdbService.GetMovieCreditsAsync(tmdbMovieId);
        //    if (credits?.cast == null) return;

        //    foreach (var c in credits.cast)
        //    {
        //        var castMember = (await _unitOfWork.Repository<CastMember>().GetAll())
        //                         .FirstOrDefault(x => x.TmdbId == c.id);

        //        if (castMember == null)
        //        {
        //            castMember = new CastMember
        //            {
        //                Id = Guid.NewGuid(),
        //                Name = c.name,
        //                PhotoUrl = c.profile_path != null ? $"https://image.tmdb.org/t/p/w500{c.profile_path}" : "",
        //                Bio = "",
        //                TmdbId = c.id
        //            };
        //            await _unitOfWork.Repository<CastMember>().Add(castMember);
        //        }

        //        movie.Castings.Add(new MovieCast
        //        {
        //            Movie = movie,
        //            CastMember = castMember,
        //            CharacterName = c.character
        //        });
        //    }


        //    await _unitOfWork.SaveChangesAsync();
        //}


        public async Task<List<MovieDto>> GetAllImportedMoviesAsync()
        {
            // 1- هات كل الأفلام من الداتابيز
            var movies = await _unitOfWork.Repository<Movie>().GetAll();

            // 2- لو مفيش أفلام رجع list فاضية
            if (movies == null || !movies.Any())
                return new List<MovieDto>();

            // 3- اعمل mapping للـ DTO
            var result = _mapper.Map<List<MovieDto>>(movies);

            return result;
        }


        public async Task<List<TvShowDto>> GetAllImportedTvshowsAsync()
        {
            // 1- هات كل الأفلام من الداتابيز
            var movies = await _unitOfWork.Repository<TVShow>().GetAll();

            // 2- لو مفيش أفلام رجع list فاضية
            if (movies == null || !movies.Any())
                return new List<TvShowDto>();

            // 3- اعمل mapping للـ DTO
            var result = _mapper.Map<List<TvShowDto>>(movies);

            return result;
        }













        public async Task<AllMediaDto> GetMediaByGenreIdAsync(Guid genreId)
        {
            // Fetch Movies based on genre
            var moviesQuery = _repo.GetAllQueryable()
                .Where(m => m.CurrentState == 1 && m.MovieGenres.Any(g => g.GenreId == genreId))
                .Include(m => m.MovieGenres)
                .ThenInclude(g => g.Genre);

            var movies = await moviesQuery.ToListAsync();
            var movieDtos = _mapper.Map<List<MovieDto>>(movies);

            // Fetch TV Shows based on genre
            var showsQuery = _showRepo.GetAllQueryable()
                .Where(s => s.CurrentState == 1 && s.TVShowGenres.Any(g => g.GenreId == genreId))
                .Include(s => s.TVShowGenres)
                .ThenInclude(g => g.Genre)
                .Include(s => s.Seasons); // Optional لو هتعرض عدد المواسم

            var shows = await showsQuery.ToListAsync();
            var showDtos = _mapper.Map<List<TvShowDto>>(shows);

            return new AllMediaDto
            {
                Movies = movieDtos,
                TvShows = showDtos
            };
        }



        // i need impelemention of this method GetAllMediaAsync that retures all movies and all Tvshows
        public async Task<AllMediaDto> GetAllMediaAsync()
        {
            var movieRepo = _unitOfWork.Repository<Movie>();
            var tvShowRepo = _unitOfWork.Repository<TVShow>();
            var movies = await movieRepo.GetList(m => m.CurrentState == 1);
            var tvShows = await tvShowRepo.GetList(t => t.CurrentState == 1);
            var moviesDto = _mapper.Map<List<MovieDto>>(movies);
            var tvShowsDto = _mapper.Map<List<TvShowDto>>(tvShows);
            return new AllMediaDto
            {
                Movies = moviesDto,
                TvShows = tvShowsDto
            };

        }

    }
}

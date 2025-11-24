using AutoMapper;
using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using InfrastructureLayer.Contracts;
using Domains;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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


        public MovieService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserService userService,
            IGenericRepository<TVShow> showRepo,
            IGenericRepository<Movie> repo

        ) : base(unitOfWork, mapper, userService)
        {
            _repo = repo;
            _showRepo = showRepo;
            _unitOfWork = unitOfWork;
            _repo = _unitOfWork.Repository<Movie>();
            _mapper = mapper;
            _userService = userService;
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
        // Get By Id
        // ===========================
        public async Task<MovieDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repo.GetById(id);
            return entity == null ? null : _mapper.Map<MovieDto>(entity);
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
        public async Task<GenreMoviesResponseDto> GetMoviesByGenreAsync(Guid genreId, int page = 1, int pageSize = 20)
        {
            // Load genre name for response metadata
            var genreRepo = _unitOfWork.Repository<Genre>();
            var genre = await genreRepo.GetById(genreId);

            // Query movies filtered by genre on the DB side and include relations needed for mapping
            var paged = await _repo.GetPagedList<Movie>(
                pageNumber: page,
                pageSize: pageSize,
                filter: m => m.CurrentState == 1 && m.MovieGenres.Any(mg => mg.GenreId == genreId),
                selector: null, // return Movie entities
                orderBy: m => m.ReleaseYear,
                isDescending: true,
                // include genres and cast for mapping enrichment
                m => m.MovieGenres,
                m => m.Castings
            );

            var moviesDto = _mapper.Map<List<MovieDto>>(paged.Items);

            return new GenreMoviesResponseDto
            {
                GenreId = genreId,
                GenreName = genre?.Name ?? string.Empty,
                Movies = moviesDto,
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
                Movies = moviesDto,
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
            var list = await _repo.GetList(m => m.CurrentState == 1 && m.IsFeatured);
            var ordered = list.OrderByDescending(m => m.CreatedDate).Take(limit).ToList();
            return _mapper.Map<IEnumerable<MovieDto>>(ordered);
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

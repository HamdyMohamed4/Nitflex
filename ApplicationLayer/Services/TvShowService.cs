using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using AutoMapper;
using Domains;
using InfrastructureLayer.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationLayer.Services
{
    public class TvShowService : BaseService<TVShow, TvShowDto>, ITvShowService
    {
        private readonly IGenericRepository<TVShow> _tvShowRepo;
        private readonly IGenericRepository<Season> _seasonRepo;
        private readonly IGenericRepository<Episode> _episodeRepo;
        private readonly IGenericRepository<Genre> _genreRepo;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;

        public TvShowService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserService userService) : base(unitOfWork, mapper, userService)
        {
            _unitOfWork = unitOfWork;
            _tvShowRepo = _unitOfWork.Repository<TVShow>();
            _seasonRepo = _unitOfWork.Repository<Season>();
            _episodeRepo = _unitOfWork.Repository<Episode>();
            _genreRepo = _unitOfWork.Repository<Genre>();
            _mapper = mapper;
            _userService = userService;
        }

        // ===========================
        // TvShow: Basic CRUD & listing
        // ===========================

        // Get all TV shows
        public async Task<List<TvShowDto>> GetAllAsync(Guid? genreId = null, string? search = null)
        {
            var list = await _tvShowRepo.GetList(s => s.CurrentState == 1);

            if (genreId.HasValue)
                list = list.Where(s => s.TVShowGenres.Any(g => g.GenreId == genreId.Value)).ToList();

            if (!string.IsNullOrWhiteSpace(search))
                list = list.Where(s => s.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var tvShow in list)
            {
                tvShow.Seasons = tvShow.Seasons
                    .Where(se => se.CurrentState == 1)
                    .OrderBy(se => se.SeasonNumber)
                    .ToList();

                foreach (var season in tvShow.Seasons)
                {
                    season.Episodes = season.Episodes
                        .Where(ep => ep.CurrentState == 1)
                        .OrderBy(ep => ep.EpisodeNumber)
                        .ToList();
                }
            }

            var orderedList = list
               .OrderByDescending(s => s.CreatedDate)
               .ToList();

            return _mapper.Map<List<TvShowDto>>(orderedList);
        }

        // Get TV show by id



        public async Task<TvShowDetailsDto?> GetByIdAsync(Guid id)
        {
            // جلب الـ TVShow مع كل الـ relations المهمة
            var shows = await _unitOfWork.Repository<TVShow>()
                .GetListWithInclude(
                    filter: x => x.Id == id,
                    include: query => query
                        .Include(x => x.Castings)
                            .ThenInclude(c => c.CastMember)
                        .Include(x => x.TVShowGenres)
                            .ThenInclude(g => g.Genre)
                        .Include(x => x.Seasons)
                            .ThenInclude(s => s.Episodes)
                );

            var show = shows.FirstOrDefault();
            if (show == null)
                return null;

            // تحديد أول 10 من الكاست
            show.Castings = show.Castings
                .Where(c => c.CastMember != null)
                .OrderBy(c => c.Id)
                .Take(10)
                .ToList();

            // ترتيب الـ Seasons والـ Episodes
            foreach (var season in show.Seasons)
            {
                season.Episodes = season.Episodes
                    .Where(e => e.CurrentState == 1)
                    .OrderBy(e => e.EpisodeNumber)
                    .ToList();
            }

            show.Seasons = show.Seasons
                .Where(s => s.CurrentState == 1)
                .OrderBy(s => s.SeasonNumber)
                .ToList();

            // Mapping للـ DTO النهائي
            var dto = _mapper.Map<TvShowDetailsDto>(show);

            return dto;
        }

        // Create TV show
        public async Task<TvShowDto> CreateAsync(CreateTvShowDto dto)
        {
            var entity = _mapper.Map<TVShow>(dto);
            entity.Id = Guid.NewGuid();
            entity.CreatedBy = _userService.GetLoggedInUser();
            entity.CreatedDate = DateTime.UtcNow;
            entity.CurrentState = 1;

            await _tvShowRepo.Add(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<TvShowDto>(entity);
        }

        // Update TV show
        public async Task<bool> UpdateAsync(Guid id, UpdateTvShowDto dto)
        {
            var entity = await _tvShowRepo.GetById(id);
            if (entity == null) return false;

            _mapper.Map(dto, entity);
            entity.UpdatedBy = _userService.GetLoggedInUser();
            entity.UpdatedDate = DateTime.UtcNow;

            var result = await _tvShowRepo.Update(entity);
            
            if (result)
                await _unitOfWork.SaveChangesAsync();

            return result;
        }
    
        // Delete TV showH
        public async Task<bool> DeleteAsync(Guid id)
        {
            var result = await _tvShowRepo.ChangeStatus(id, _userService.GetLoggedInUser(), 0);

            if (result)
                await _unitOfWork.SaveChangesAsync();

            return result;
        }


        // ===========================
        // Genre-based listing
        // ===========================
        public async Task<GenreShowsResponseDto> GetShowsByGenreAsync(Guid genreId, int page = 1, int pageSize = 20)
        {
            var genre = await _genreRepo.GetById(genreId);

            var paged = await _tvShowRepo.GetPagedList<TVShow>(
                pageNumber: page,
                pageSize: pageSize,
                filter: s => s.CurrentState == 1 && s.TVShowGenres.Any(g => g.GenreId == genreId),
                selector: null,
                orderBy: s => s.CreatedDate,
                isDescending: true,
                s => s.Seasons,
                s => s.TVShowGenres,
                s => s.Castings
            );

            var shows = _mapper.Map<List<TvShowDto>>(paged.Items);

            return new GenreShowsResponseDto
            {
                GenreId = genreId,
                GenreName = genre?.Name ?? string.Empty,
                MediaData = shows,
                TotalCount = paged.TotalCount,
                Page = page,
                PageSize = pageSize
            };
        }

 


        public async Task<IEnumerable<TvShowDto>> GetFeaturedAsync(int limit = 10)
        {
            var tvShows = await _unitOfWork.Repository<TVShow>()
                .GetListWithInclude(
                    filter: m => m.CurrentState == 1 && m.IsFeatured,
                    include: query => query
                        .Include(x => x.TVShowGenres)
                            .ThenInclude(mg => mg.Genre)
                        .Include(x => x.Castings)
                            .ThenInclude(c => c.CastMember)
                        .Include(x => x.Seasons)
                                .ThenInclude(s => s.Episodes)

                );

            var ordered = tvShows
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

            return _mapper.Map<IEnumerable<TvShowDto>>(ordered);
        }





        //public async Task<IEnumerable<TvShowDto>> GetAllShowsAsync()
        //{
        //    var shows = await _unitOfWork.Repository<TVShow>()
        //        .GetListWithInclude(
        //            filter: x => true,
        //            include: query => query
        //                .Include(x => x.TVShowGenres)
        //                    .ThenInclude(mg => mg.Genre)
        //                .Include(x => x.Castings)
        //                    .ThenInclude(c => c.CastMember)
        //                .Include(x => x.Seasons)
        //                    .ThenInclude(s => s.Episodes)
        //        );

        //    return _mapper.Map<IEnumerable<TvShowDto>>(shows);
        //}






        public async Task<IEnumerable<TvShowDto>> GetAllTvShowsAsync()
        {
            // Load TV Shows with Related Entities
            var shows = await _unitOfWork.Repository<TVShow>()
                .GetListWithInclude(
                    filter: x => true,
                    include: query => query
                        .Include(x => x.Castings)
                            .ThenInclude(c => c.CastMember)
                        .Include(x => x.TVShowGenres)
                            .ThenInclude(g => g.Genre)
                        .Include(x => x.Seasons)
                            .ThenInclude(s => s.Episodes)
                );

            // Map to DTO
            var result = shows.Select(m => new TvShowDto
            {
                Name = m.Name,
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseYear = m.ReleaseYear,
                DurationMinutes = m.DurationMinutes,
                AgeRating = m.AgeRating,
                PosterUrl = m.PosterUrl,
                TmdbId = m.TmdbId,
                IsFeatured = m.IsFeatured,
                Type = m.Type,
                Language = m.Language,
                AudioType = m.AudioType,

                // Genres
                GenresNames = m.TVShowGenres.Select(mg => new GenreDto
                {
                    Id = mg.Genre.Id,
                    Name = mg.Genre.Name
                }).ToList(),

                // Cast (first 10 only)
                Cast = m.Castings
                    .Where(c => c.CastMember != null)
                    .OrderBy(c => c.Id)
                    .Take(10)
                    .Select(c => new CastDto
                    {
                        Id = c.CastMember.Id,
                        Name = c.CastMember.Name,
                        CharacterName = c.CharacterName
                    }).ToList(),

                // Seasons + Episodes
                Seasons = m.Seasons
                    .OrderBy(s => s.SeasonNumber)
                    .Select(s => new SeasonWithEpisodesDto
                    {
                        SeasonNumber = s.SeasonNumber,
                        Episodes = s.Episodes
                            .OrderBy(e => e.EpisodeNumber)
                            .Select(e => new EpisodeDto
                            {
                                Id = e.Id,
                                Title = e.Title,
                                Duration = e.DurationMinutes,
                                EpisodeNumber = e.EpisodeNumber,
                                VideoUrl = e.VideoUrl,
                                TrailerUrl = e.TrailerUrl,

                            }).ToList()
                    }).ToList()
            }).ToList();

            return result;
        }




        public async Task<GenreShowsResponseDto?> GetShowsByGenreNameAsync(string genreName, int page = 1, int pageSize = 20)
        {
            // First get the genre by name (case insensitive)
            var genre = await _genreRepo.GetFirstOrDefault(g => g.Name.ToLower() == genreName.ToLower());

            if (genre == null)
                return null;

            var paged = await _tvShowRepo.GetPagedList<TVShow>(
                pageNumber: page,
                pageSize: pageSize,
                filter: s => s.CurrentState == 1 && s.TVShowGenres.Any(g => g.GenreId == genre.Id),
                selector: null,
                orderBy: s => s.CreatedDate,
                isDescending: true,

                // INCLUDED RELATIONSHIPS (correct way)
                s => s.Seasons,
                s => s.Seasons.Select(season => season.Episodes),
                s => s.TVShowGenres,
                s => s.Castings
            );

            var shows = _mapper.Map<List<TvShowDto>>(paged.Items);

            return new GenreShowsResponseDto
            {
                GenreId = genre.Id,
                GenreName = genre.Name,
                MediaData = shows,
                TotalCount = paged.TotalCount,
                Page = page,
                PageSize = pageSize
            };
        }



        public async Task<IEnumerable<TvShowDto>> GetByGenreIdsAsync(IEnumerable<Guid> genreIds, int limit = 50)
        {
            var ids = genreIds?.ToList() ?? new List<Guid>();
            if (!ids.Any()) return Enumerable.Empty<TvShowDto>();

            var list = await _tvShowRepo.GetList(s => s.CurrentState == 1 && s.TVShowGenres.Any(g => ids.Contains(g.GenreId)));
            var ordered = list.OrderByDescending(s => s.CreatedDate).Take(limit).ToList();
            return _mapper.Map<IEnumerable<TvShowDto>>(ordered);
        }

        public async Task<string?> GetStreamingUrlAsync(Guid tvShowId, Guid profileId)
        {
            var show = await _tvShowRepo.GetById(tvShowId);
            if (show == null) return null;

            // Placeholder: return BannerUrl or PosterUrl as a simple locator.
            // Replace with signed CDN/DRM locator and subscription checks when available.
            return !string.IsNullOrWhiteSpace(show.Name) ? show.PosterUrl : show.PosterUrl;
        }


        // ===========================
        // Seasons
        // ===========================

        // Get season by id
        public async Task<SeasonDto?> GetSeasonByIdAsync(Guid seasonId)
        {
            var season = await _seasonRepo.GetById(seasonId);

            if (season == null) return null;

            var episodes = await _episodeRepo.GetList(
                e => e.SeasonId == seasonId && e.CurrentState == 1
            );

            season.Episodes = episodes.OrderBy(e => e.EpisodeNumber).ToList();

            return _mapper.Map<SeasonDto>(season);
        }

        // Get all seasons for series
        public async Task<List<SeasonDto>> GetAllSeasonsByTvShowIdAsync(Guid tvShowId)
        {
            var seasons = await _seasonRepo.GetList(
                s => s.TvShowId == tvShowId && s.CurrentState == 1
            );

            if (!seasons.Any())
                return new List<SeasonDto>();

            await Task.WhenAll(seasons.Select(async season =>
            {
                var episodes = await _episodeRepo.GetList(
                    e => e.SeasonId == season.Id && e.CurrentState == 1
                );
                season.Episodes = episodes.OrderBy(e => e.EpisodeNumber).ToList();
            }));

            var orderedSeasons = seasons.OrderBy(s => s.SeasonNumber).ToList();

            return _mapper.Map<List<SeasonDto>>(orderedSeasons);
        }
        
        // Create season
        public async Task<SeasonDto?> CreateSeasonAsync(Guid tvShowId, CreateSeasonDto dto)
        {
            var show = await _tvShowRepo.GetById(tvShowId);
            if (show == null) throw new InvalidOperationException("TV show not found.");

            var season = _mapper.Map<Season>(dto);
            season.Id = Guid.NewGuid();
            season.TvShowId = tvShowId;
            season.CreatedBy = _userService.GetLoggedInUser();
            season.CreatedDate = DateTime.UtcNow;
            season.CurrentState = 1;

            await _seasonRepo.Add(season);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<SeasonDto>(season);
        }

        // Update season
        public async Task<bool> UpdateSeasonAsync(Guid tvShowId, Guid seasonId, UpdateSeasonDto dto)
        {
            var season = await _seasonRepo.GetById(seasonId);
            if (season == null || season.TvShowId != tvShowId) return false;

            _mapper.Map(dto, season);
            season.UpdatedBy = _userService.GetLoggedInUser();
            season.UpdatedDate = DateTime.UtcNow;

            var result = await _seasonRepo.Update(season);
            
            if (result)
                await _unitOfWork.SaveChangesAsync();

            return result;
        }

        // Delete season
        public async Task<bool> DeleteSeasonAsync(Guid tvShowId, Guid seasonId)
        {
            var season = await _seasonRepo.GetById(seasonId);
            if (season == null || season.TvShowId != tvShowId) return false;

            var result = await _seasonRepo.ChangeStatus(seasonId, _userService.GetLoggedInUser(), 0);

            if (result)
                await _unitOfWork.SaveChangesAsync();

            return result;
        }


        // ===========================
        // Episodes
        // ===========================

        // Get episode by id
        public async Task<EpisodeDto?> GetEpisodeByIdAsync(Guid episodeId)
        {
            var episode = await _episodeRepo.GetById(episodeId);
            return episode == null ? null : _mapper.Map<EpisodeDto>(episode);
        }

        // Get all episodes for season
        public async Task<List<EpisodeDto>> GetAllEpisodesBySeasonIdAsync(Guid seasonId)
        {
            var episodes = await _episodeRepo.GetList(e => e.SeasonId == seasonId && e.CurrentState == 1);

            var orderedEpisodes = episodes
               .OrderBy(e => e.EpisodeNumber) 
               .ToList();

            return _mapper.Map<List<EpisodeDto>>(orderedEpisodes);
        }

        // Get all episodes for tv show
        public async Task<List<EpisodeDto>> GetAllEpisodesByTvShowIdAsync(Guid tvShowId)
        {

            var episodes = await _episodeRepo.GetList(
                e => e.Season.TvShowId == tvShowId
                  && e.CurrentState == 1
                  && e.Season.CurrentState == 1
            );

            var orderedEpisodes = episodes
                .OrderBy(e => e.Season.SeasonNumber)
                .ThenBy(e => e.EpisodeNumber)
                .ToList();

            return _mapper.Map<List<EpisodeDto>>(orderedEpisodes);
        }

        // Create episode
        public async Task<EpisodeDto?> CreateEpisodeAsync(Guid tvShowId, Guid seasonId, CreateEpisodeDto dto)
        {
            var season = await _seasonRepo.GetById(seasonId);
            if (season == null || season.TvShowId != tvShowId) throw new InvalidOperationException("Season not found or mismatched show.");

            var episode = _mapper.Map<Episode>(dto);
            episode.Id = Guid.NewGuid();
            episode.SeasonId = seasonId;
            episode.CreatedBy = _userService.GetLoggedInUser();
            episode.CreatedDate = DateTime.UtcNow;
            episode.CurrentState = 1;

            await _episodeRepo.Add(episode);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<EpisodeDto>(episode);
        }

        // Update episode
        public async Task<bool> UpdateEpisodeAsync(Guid tvShowId, Guid seasonId, Guid episodeId, UpdateEpisodeDto dto)
        {
            var episode = await _episodeRepo.GetById(episodeId);
            if (episode == null || episode.SeasonId != seasonId) return false;

            // verify season belongs to tvShow
            var season = await _seasonRepo.GetById(seasonId);
            if (season == null || season.TvShowId != tvShowId) return false;

            _mapper.Map(dto, episode);
            episode.UpdatedBy = _userService.GetLoggedInUser();
            episode.UpdatedDate = DateTime.UtcNow;

            var result = await _episodeRepo.Update(episode);

            if (result)
                await _unitOfWork.SaveChangesAsync();

            return result;
        }
        
        // Delete episode
        public async Task<bool> DeleteEpisodeAsync(Guid tvShowId, Guid seasonId, Guid episodeId)
        {
            var episode = await _episodeRepo.GetById(episodeId);
            if (episode == null || episode.SeasonId != seasonId) return false;

            var season = await _seasonRepo.GetById(seasonId);
            if (season == null || season.TvShowId != tvShowId) return false;

            var result = await _episodeRepo.ChangeStatus(episodeId, _userService.GetLoggedInUser(), 0);
            
            if (result)
                await _unitOfWork.SaveChangesAsync();

            return result;
        }

    }
}

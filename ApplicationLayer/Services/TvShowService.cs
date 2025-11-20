using AutoMapper;
using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using InfrastructureLayer.Contracts;
using Domains;
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
        public async Task<List<TvShowDto>> GetAllAsync(Guid? genreId = null, string? search = null)
        {
            var list = await _tvShowRepo.GetList(s => s.CurrentState == 1);

            if (genreId.HasValue)
                list = list.Where(s => s.TVShowGenres.Any(g => g.GenreId == genreId.Value)).ToList();

            if (!string.IsNullOrWhiteSpace(search))
                list = list.Where(s => s.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            return _mapper.Map<List<TvShowDto>>(list);
        }

        public async Task<TvShowDetailsDto?> GetByIdAsync(Guid id)
        {
            var tvShow = await _tvShowRepo.GetById(id);
            if (tvShow == null) return null;

            // load seasons and their episodes
            var seasons = await _seasonRepo.GetList(s => s.TvShowId == id);
            foreach (var season in seasons)
            {
                var episodes = await _episodeRepo.GetList(e => e.SeasonId == season.Id);
                season.Episodes = episodes;
            }

            var dto = _mapper.Map<TvShowDetailsDto>(tvShow);
            dto.Seasons = _mapper.Map<List<SeasonDto>>(seasons);

            return dto;
        }

        public async Task<TvShowDto> CreateAsync(CreateTvShowDto dto)
        {
            var entity = _mapper.Map<TVShow>(dto);
            entity.Id = Guid.NewGuid();
            entity.CreatedBy = _userService.GetLoggedInUser();
            entity.CreatedDate = DateTime.UtcNow;
            entity.CurrentState = 1;

            await _tvShowRepo.Add(entity);
            return _mapper.Map<TvShowDto>(entity);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateTvShowDto dto)
        {
            var entity = await _tvShowRepo.GetById(id);
            if (entity == null) return false;

            _mapper.Map(dto, entity);
            entity.UpdatedBy = _userService.GetLoggedInUser();
            entity.UpdatedDate = DateTime.UtcNow;

            return await _tvShowRepo.Update(entity);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _tvShowRepo.ChangeStatus(id, _userService.GetLoggedInUser(), 0);
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
                Shows = shows,
                TotalCount = paged.TotalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<TvShowDto>> GetFeaturedAsync(int limit = 10)
        {
            var list = await _tvShowRepo.GetList(s => s.CurrentState == 1);
            var ordered = list.OrderByDescending(s => s.CreatedDate).Take(limit).ToList();
            return _mapper.Map<IEnumerable<TvShowDto>>(ordered);
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
            return !string.IsNullOrWhiteSpace(show.BannerUrl) ? show.BannerUrl : show.PosterUrl;
        }

        // ===========================
        // Seasons
        // ===========================
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
            return _mapper.Map<SeasonDto>(season);
        }

        public async Task<bool> UpdateSeasonAsync(Guid tvShowId, Guid seasonId, UpdateSeasonDto dto)
        {
            var season = await _seasonRepo.GetById(seasonId);
            if (season == null || season.TvShowId != tvShowId) return false;

            _mapper.Map(dto, season);
            season.UpdatedBy = _userService.GetLoggedInUser();
            season.UpdatedDate = DateTime.UtcNow;

            return await _seasonRepo.Update(season);
        }

        public async Task<bool> DeleteSeasonAsync(Guid tvShowId, Guid seasonId)
        {
            var season = await _seasonRepo.GetById(seasonId);
            if (season == null || season.TvShowId != tvShowId) return false;

            return await _seasonRepo.ChangeStatus(seasonId, _userService.GetLoggedInUser(), 0);
        }

        // ===========================
        // Episodes
        // ===========================
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
            return _mapper.Map<EpisodeDto>(episode);
        }

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

            return await _episodeRepo.Update(episode);
        }

        public async Task<bool> DeleteEpisodeAsync(Guid tvShowId, Guid seasonId, Guid episodeId)
        {
            var episode = await _episodeRepo.GetById(episodeId);
            if (episode == null || episode.SeasonId != seasonId) return false;

            var season = await _seasonRepo.GetById(seasonId);
            if (season == null || season.TvShowId != tvShowId) return false;

            return await _episodeRepo.ChangeStatus(episodeId, _userService.GetLoggedInUser(), 0);
        }
    }
}

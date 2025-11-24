using ApplicationLayer.Dtos;
using Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    /// <summary>
    /// Service contract for TV show operations (listing, details, seasons, episodes, playback).
    /// </summary>
    public interface ITvShowService : IBaseService<TVShow, TvShowDto>
    {
        //// TvShow
        Task<List<TvShowDto>> GetAllAsync(Guid? genreId = null, string? search = null);
        Task<TvShowDetailsDto?> GetByIdAsync(Guid id);
        Task<TvShowDto> CreateAsync(CreateTvShowDto dto);
        Task<bool> UpdateAsync(Guid id, UpdateTvShowDto dto);
        Task<bool> DeleteAsync(Guid id);

        // Genre-based listing (paged) and helpers
        Task<GenreShowsResponseDto> GetShowsByGenreAsync(Guid genreId, int page = 1, int pageSize = 20);
        Task<IEnumerable<TvShowDto>> GetFeaturedAsync(int limit = 10);
        Task<IEnumerable<TvShowDto>> GetByGenreIdsAsync(IEnumerable<Guid> genreIds, int limit = 50);

        // Streaming / playback locator (may validate profile/subscription)
        Task<string?> GetStreamingUrlAsync(Guid tvShowId, Guid profileId);

        // Seasons
        Task<SeasonDto?> GetSeasonByIdAsync(Guid seasonId);
        Task<List<SeasonDto>> GetAllSeasonsByTvShowIdAsync(Guid tvShowId);
        Task<SeasonDto?> CreateSeasonAsync(Guid tvShowId, CreateSeasonDto dto);
        Task<bool> UpdateSeasonAsync(Guid tvShowId, Guid seasonId, UpdateSeasonDto dto);
        Task<bool> DeleteSeasonAsync(Guid tvShowId, Guid seasonId);

        // Episodes
        Task<EpisodeDto?> GetEpisodeByIdAsync(Guid episodeId);
        Task<List<EpisodeDto>> GetAllEpisodesBySeasonIdAsync(Guid seasonId);
        Task<List<EpisodeDto>> GetAllEpisodesByTvShowIdAsync(Guid tvShowId);
        Task<EpisodeDto?> CreateEpisodeAsync(Guid tvShowId, Guid seasonId, CreateEpisodeDto dto);
        Task<bool> UpdateEpisodeAsync(Guid tvShowId, Guid seasonId, Guid episodeId, UpdateEpisodeDto dto);
        Task<bool> DeleteEpisodeAsync(Guid tvShowId, Guid seasonId, Guid episodeId);
    }
}

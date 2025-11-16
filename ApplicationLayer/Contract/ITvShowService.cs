using ApplicationLayer.Dtos;
using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    public interface ITvShowService : IBaseService<TVShow, TvShowDto>
    {
        // TvShow
        Task<List<TvShowDto>> GetAllAsync(Guid? genreId = null, string? search = null);
        Task<TvShowDetailsDto?> GetByIdAsync(Guid id);
        Task<TvShowDto> CreateAsync(CreateTvShowDto dto);
        Task<bool> UpdateAsync(Guid id, UpdateTvShowDto dto);
        Task<bool> DeleteAsync(Guid id);

        // Seasons
        Task<SeasonDto?> CreateSeasonAsync(Guid tvShowId, CreateSeasonDto dto);
        Task<bool> UpdateSeasonAsync(Guid tvShowId, Guid seasonId, UpdateSeasonDto dto);
        Task<bool> DeleteSeasonAsync(Guid tvShowId, Guid seasonId);

        // Episodes
        Task<EpisodeDto?> CreateEpisodeAsync(Guid tvShowId, Guid seasonId, CreateEpisodeDto dto);
        Task<bool> UpdateEpisodeAsync(Guid tvShowId, Guid seasonId, Guid episodeId, UpdateEpisodeDto dto);
        Task<bool> DeleteEpisodeAsync(Guid tvShowId, Guid seasonId, Guid episodeId);
    }
}

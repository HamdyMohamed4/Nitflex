using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminTvShowController : ControllerBase
    {
        private readonly ITvShowService _tvShowService;

        public AdminTvShowController(ITvShowService tvShowService)
        {
            _tvShowService = tvShowService;
        }


        // ===========================
        // TVShow CRUD
        // ===========================

        // GET: api/AdminTvShow
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<TvShowDto>>>> GetAll()
        {
            try
            {
                var shows = await _tvShowService.GetAllAsync();

                if (shows == null || !shows.Any())
                    return NotFound(ApiResponse<List<TvShowDto>>.FailResponse("No shows found."));

                return Ok(ApiResponse<List<TvShowDto>>.SuccessResponse(shows.ToList(), "Shows retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TvShowDto>>.FailResponse("Failed to retrieve shows.", new List<string> { ex.Message }));
            }
        }


        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<TvShowDto>>> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<TvShowDto>.FailResponse("Invalid show id."));

                var serviceResponse = await _tvShowService.GetByIdAsync(id);

                if (serviceResponse == null || serviceResponse.Data == null)
                    return NotFound(ApiResponse<TvShowDto>.FailResponse("Show not found."));

                return Ok(ApiResponse<TvShowDto>.SuccessResponse(serviceResponse.Data, serviceResponse.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TvShowDto>.FailResponse("Failed to retrieve show", new List<string> { ex.Message }));
            }
        }

        // POST: api/AdminTvShow
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TvShowDto>>> Create([FromBody] CreateTvShowDto dto)
        {
            try
            {
                var created = await _tvShowService.CreateAsync(dto);

                return Ok(ApiResponse<TvShowDto>.SuccessResponse(created, "Show created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TvShowDto>.FailResponse("Failed to create show.", new List<string> { ex.Message }));
            }
        }

        // PUT: api/AdminTvShow/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Update(Guid id, [FromBody] UpdateTvShowDto dto)
        {
            try
            {
                var updated = await _tvShowService.UpdateAsync(id, dto);

                if (!updated)
                    return NotFound(ApiResponse<bool>.FailResponse("Show not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(updated, "Show updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Failed to update show.", new List<string> { ex.Message }));
            }
        }

        // DELETE: api/AdminTvShow/{id}
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
        {
            try
            {
                var result = await _tvShowService.DeleteAsync(id);

                if (!result)
                    return NotFound(ApiResponse<bool>.FailResponse("Show not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Show deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Failed to delete show.", new List<string> { ex.Message }));
            }
        }



        // ===========================
        // Seasons CRUD
        // ===========================

        // GET: api/AdminTvShow/seasons/{seasonId}
        [HttpGet("seasons/{seasonId:guid}")]
        public async Task<ActionResult<ApiResponse<SeasonDto>>> GetSeasonById(Guid seasonId)
        {
            try
            {
                var season = await _tvShowService.GetSeasonByIdAsync(seasonId);

                if (season == null)
                    return NotFound(ApiResponse<SeasonDto>.FailResponse("Season not found."));

                return Ok(ApiResponse<SeasonDto>.SuccessResponse(season, "Season retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<SeasonDto>.FailResponse("Failed to retrieve season.", new List<string> { ex.Message }));
            }
        }

        // GET: api/AdminTvShow/{tvShowId}/seasons
        [HttpGet("{tvShowId:guid}/seasons")]
        public async Task<ActionResult<ApiResponse<List<SeasonDto>>>> GetSeasonsByTvShow(Guid tvShowId)
        {
            try
            {
                var seasons = await _tvShowService.GetAllSeasonsByTvShowIdAsync(tvShowId);

                if (seasons == null || !seasons.Any())
                    return NotFound(ApiResponse<List<SeasonDto>>.FailResponse("No seasons found."));

                return Ok(ApiResponse<List<SeasonDto>>.SuccessResponse(seasons, "Seasons retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<SeasonDto>>.FailResponse("Failed to retrieve seasons.", new List<string> { ex.Message }));
            }
        }

        // POST: api/AdminTvShow/{tvShowId}/seasons
        [HttpPost("{tvShowId:guid}/seasons")]
        public async Task<ActionResult<ApiResponse<SeasonDto>>> CreateSeason(Guid tvShowId, [FromBody] CreateSeasonDto dto)
        {
            try
            {
                var season = await _tvShowService.CreateSeasonAsync(tvShowId, dto);

                return Ok(ApiResponse<SeasonDto>.SuccessResponse(season, "Season created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<SeasonDto>.FailResponse("Failed to create season.", new List<string> { ex.Message }));
            }
        }

        // PUT: api/AdminTvShow/{tvShowId}/seasons/{seasonId}
        [HttpPut("{tvShowId:guid}/seasons/{seasonId:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateSeason(Guid tvShowId, Guid seasonId, [FromBody] UpdateSeasonDto dto)
        {
            try
            {
                var update = await _tvShowService.UpdateSeasonAsync(tvShowId, seasonId, dto);

                if (!update)
                    return NotFound(ApiResponse<bool>.FailResponse("Season not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(update, "Season updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Failed to update season.", new List<string> { ex.Message }));
            }
        }

        // DELETE: api/AdminTvShow/{tvShowId}/seasons/{seasonId}
        [HttpDelete("{tvShowId:guid}/seasons/{seasonId:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteSeason(Guid tvShowId, Guid seasonId)
        {
            try
            {
                var result = await _tvShowService.DeleteSeasonAsync(tvShowId, seasonId);

                if (!result)
                    return NotFound(ApiResponse<bool>.FailResponse("Season not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Season deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Failed to delete season.", new List<string> { ex.Message }));
            }
        }



        // ===========================
        // Episodes CRUD
        // ===========================

        // GET: api/AdminTvShow/episodes/{episodeId}
        [HttpGet("episodes/{episodeId:guid}")]
        public async Task<ActionResult<ApiResponse<EpisodeDto>>> GetEpisodeById(Guid episodeId)
        {
            try
            {
                var episode = await _tvShowService.GetEpisodeByIdAsync(episodeId);

                if (episode == null)
                    return NotFound(ApiResponse<EpisodeDto>.FailResponse("Episode not found."));

                return Ok(ApiResponse<EpisodeDto>.SuccessResponse(episode, "Episode retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<EpisodeDto>.FailResponse("Failed to retrieve episode.", new List<string> { ex.Message }));
            }
        }

        // GET: api/AdminTvShow/{seasonId:guid}/episodes
        [HttpGet("{seasonId:guid}/episodes")]
        public async Task<ActionResult<ApiResponse<List<EpisodeDto>>>> GetEpisodesBySeason(Guid seasonId)
        {
            try
            {
                var episodes = await _tvShowService.GetAllEpisodesBySeasonIdAsync(seasonId);

                if (episodes == null || !episodes.Any())
                    return NotFound(ApiResponse<List<EpisodeDto>>.FailResponse("No episodes found."));

                return Ok(ApiResponse<List<EpisodeDto>>.SuccessResponse(episodes, "Episodes retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<EpisodeDto>>.FailResponse("Failed to retrieve episodes.", new List<string> { ex.Message }));
            }
        }

      

        // POST: api/AdminTvShow/{tvShowId}/seasons/{seasonId}/episodes
        [HttpPost("{tvShowId:guid}/seasons/{seasonId:guid}/episodes")]
        public async Task<ActionResult<ApiResponse<EpisodeDto>>> CreateEpisode(Guid tvShowId, Guid seasonId, [FromBody] CreateEpisodeDto dto)
        {
            try
            {
                var episode = await _tvShowService.CreateEpisodeAsync(tvShowId, seasonId, dto);

                return Ok(ApiResponse<EpisodeDto>.SuccessResponse(episode, "Episode created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<EpisodeDto>.FailResponse("Failed to create episode.", new List<string> { ex.Message }));
            }
        }

        // PUT: api/AdminTvShow/{tvShowId}/seasons/{seasonId}/episodes/{episodeId}
        [HttpPut("{tvShowId:guid}/seasons/{seasonId:guid}/episodes/{episodeId:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateEpisode(Guid tvShowId, Guid seasonId, Guid episodeId, [FromBody] UpdateEpisodeDto dto)
        {
            try
            {
                var update = await _tvShowService.UpdateEpisodeAsync(tvShowId, seasonId, episodeId, dto);

                if (!update)
                    return NotFound(ApiResponse<bool>.FailResponse("Episode not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(update, "Episode updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Failed to update episode.", new List<string> { ex.Message }));
            }
        }

        // DELETE: api/AdminTvShow/{tvShowId}/seasons/{seasonId}/episodes/{episodeId}
        [HttpDelete("{tvShowId:guid}/seasons/{seasonId:guid}/episodes/{episodeId:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteEpisode(Guid tvShowId, Guid seasonId, Guid episodeId)
        {
            try
            {
                var result = await _tvShowService.DeleteEpisodeAsync(tvShowId, seasonId, episodeId);

                if (!result)
                    return NotFound(ApiResponse<bool>.FailResponse("Episode not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Episode deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Failed to delete episode.", new List<string> { ex.Message }));
            }
        }
    }
}

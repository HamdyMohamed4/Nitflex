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
    public class TvShowsController : ControllerBase
    {
        private readonly ITvShowService _tvShowService;
        private readonly IUserService _userService;

        public TvShowsController(ITvShowService tvShowService, IUserService userService)
        {
            _tvShowService = tvShowService;
            _userService = userService;
        }


        // GET: api/TvShows/genre/{genreId}?page=1&pageSize=20
        [HttpGet("genre/{genreId:guid}")]
        public async Task<ActionResult<ApiResponse<GenreShowsResponseDto>>> GetByGenre(Guid genreId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (genreId == Guid.Empty)
                    return BadRequest(ApiResponse<GenreShowsResponseDto>.FailResponse("Invalid genre id."));

                var result = await _tvShowService.GetShowsByGenreAsync(genreId, page, pageSize);

                if (result == null || result.Shows == null || !result.Shows.Any())
                    return NotFound(ApiResponse<GenreShowsResponseDto>.FailResponse("No shows found for the specified genre."));

                return Ok(ApiResponse<GenreShowsResponseDto>.SuccessResponse(result, "Shows by genre retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<GenreShowsResponseDto>.FailResponse("Failed to retrieve shows by genre", new List<string> { ex.Message }));
            }
        }







        // GET: api/TvShows/{id}/play
        // Returns a streaming locator for the show. Requires authenticated user.
        [HttpGet("{id:guid}/play")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ApiResponse<string>>> Play(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<string>.FailResponse("Invalid show id."));

                var profileId = _userService.GetLoggedInUser();

                var url = await _tvShowService.GetStreamingUrlAsync(id, profileId);

                if (string.IsNullOrWhiteSpace(url))
                    return NotFound(ApiResponse<string>.FailResponse("Streaming URL not available for this show."));

                return Ok(ApiResponse<string>.SuccessResponse(url, "Streaming URL retrieved."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.FailResponse("Failed to get streaming URL", new List<string> { ex.Message }));
            }
        }

        // GET: api/TvShows/featured?limit=10
        [HttpGet("featured")]
        public async Task<ActionResult<ApiResponse<List<TvShowDto>>>> GetFeatured([FromQuery] int limit = 10)
        {
            try
            {
                var shows = await _tvShowService.GetFeaturedAsync(limit);
                if (shows == null || !shows.Any())
                    return NotFound(ApiResponse<List<TvShowDto>>.FailResponse("No featured shows found."));

                return Ok(ApiResponse<List<TvShowDto>>.SuccessResponse(shows.ToList(), "Featured shows retrieved."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TvShowDto>>.FailResponse("Failed to retrieve featured shows", new List<string> { ex.Message }));
            }
        }
    }
}





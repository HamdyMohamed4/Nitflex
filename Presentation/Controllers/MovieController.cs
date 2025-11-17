using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
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
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        // POST: api/Movies/search
        [HttpPost("search")]
        public async Task<ActionResult<ApiResponse<List<MovieDto>>>> Search([FromBody] MovieSearchFilterDto filter)
        {
            try
            {
                // ===== Validate the filter =====
                if (filter == null)
                {
                    return BadRequest(ApiResponse<List<MovieDto>>.FailResponse(
                        "Filter data is required."
                    ));
                }

                // ===== Execute the search =====
                var movies = await _movieService.GetAllByFilter(filter);

                // ===== Check if any movies found =====
                if (movies == null || !movies.Any())
                {
                    return NotFound(ApiResponse<List<MovieDto>>.FailResponse(
                        "No movies found matching your search criteria."
                    ));
                }

                // ===== Return the results successfully =====
                return Ok(ApiResponse<List<MovieDto>>.SuccessResponse(
                    movies.ToList(),
                    "Movies retrieved successfully."
                ));
            }
            catch (Exception ex)
            {
                // ===== Catch any unexpected errors =====
                return BadRequest(ApiResponse<List<MovieDto>>.FailResponse(
                    "An error occurred while searching for movies.", new List<string> { ex.Message }
                ));
            }
        }
    }
}
using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using Presentation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminMovieController : ControllerBase
    {
        private readonly IMovieService _movieService;


        public AdminMovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        // ===========================
        // Movie CRUD
        // ===========================

        // GET: api/AdminMovie
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<MovieDto>>>> GetAll()
        {
            try
            {
                var movies = await _movieService.GetAllAsync();

                if (movies == null || !movies.Any())
                   return NotFound(ApiResponse<List<MovieDto>>.FailResponse("No movies found."));

                return Ok(ApiResponse<List<MovieDto>>.SuccessResponse(movies.ToList(), "Movies retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<MovieDto>>.FailResponse("Failed to retrieve movies.", new List<string> { ex.Message }));
            }
        }


        // GET: api/AdminMovie/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<MovieDto>>> GetById(Guid id)
        {
            try
            {
                var movie = await _movieService.GetByIdAsync(id);

                if (movie == null)
                    return NotFound(ApiResponse<MovieDto>.FailResponse("Movie not found."));

                return Ok(ApiResponse<MovieDto>.SuccessResponse(movie, "Movie retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<MovieDto>.FailResponse("Failed to retrieve movie.", new List<string> { ex.Message }));
            }
        }


        // POST: api/AdminMovie
        [HttpPost]
        public async Task<ActionResult<ApiResponse<MovieDto>>> Create([FromBody] MovieDto dto)
        {
            try
            {
                var created = await _movieService.CreateAsync(dto);

                return Ok(ApiResponse<MovieDto>.SuccessResponse(created, "Movie created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<MovieDto>.FailResponse("Failed to create movie.", new List<string> { ex.Message }));
            }
        }

        // PUT: api/AdminMovie/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Update(Guid id, [FromBody] MovieDto dto)
        {
            try
            {
                var update = await _movieService.UpdateAsync(id, dto);

                if (!update)
                    return NotFound(ApiResponse<bool>.FailResponse("Movie not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(update, "Movie updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Failed to update movie.", new List<string> { ex.Message }));
            }
        }

        // DELETE: api/AdminMovie/{id}
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
        {
            try
            {
                var success = await _movieService.DeleteAsync(id);

                if (!success)
                    return NotFound(ApiResponse<bool>.FailResponse("Movie not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(success, "Movie deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Failed to delete movie.", new List<string> { ex.Message }));
            }
        }


    }
}

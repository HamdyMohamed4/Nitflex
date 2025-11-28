using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class GenreController : ControllerBase
    {
        private readonly IGenreService _service;

        public GenreController(IGenreService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<GenreDto>>>> GetAll()
        {
            try
            {
                var genres = await _service.GetAllAsync();
                return Ok(ApiResponse<List<GenreDto>>.SuccessResponse(genres));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<GenreDto>>.FailResponse("Failed to get genres", new List<string> { ex.Message }));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<GenreDto>>> GetById(Guid id)
        {
            try
            {
                var genre = await _service.GetByIdAsync(id);
                if (genre == null)
                    return NotFound(ApiResponse<GenreDto>.FailResponse("Genre not found"));

                return Ok(ApiResponse<GenreDto>.SuccessResponse(genre));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<GenreDto>.FailResponse("Failed to get genre", new List<string> { ex.Message }));
            }
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<GenreDto>>> Create([FromBody] GenreDto dto)
        {
            try
            {
                var genre = await _service.CreateAsync(dto);
                return Ok(ApiResponse<GenreDto>.SuccessResponse(genre, "Genre created successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<GenreDto>.FailResponse("Failed to create genre", new List<string> { ex.Message }));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<GenreDto>>> Update(Guid id, [FromBody] GenreDto dto)
        {
            try
            {
                var genre = await _service.UpdateAsync(id, dto);
                if (genre == null)
                    return NotFound(ApiResponse<GenreDto>.FailResponse("Genre not found"));

                return Ok(ApiResponse<GenreDto>.SuccessResponse(genre, "Genre updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<GenreDto>.FailResponse("Failed to update genre", new List<string> { ex.Message }));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result)
                    return NotFound(ApiResponse<bool>.FailResponse("Genre not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Genre deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Failed to delete genre", new List<string> { ex.Message }));
            }
        }
    }
}

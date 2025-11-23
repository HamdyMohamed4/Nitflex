using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Domains;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;

namespace Presentation.Controllers;
[Route("api/[controller]")]
[ApiController]
public class BrowseController : ControllerBase
{
    private readonly IMovieService _movieService;

    public BrowseController(IMovieService movieService)
    {
        _movieService = movieService;
    }


    // POST: api/Movie/search
    [HttpPost("search")]
    public async Task<ActionResult<ApiResponse<List<MovieDto>>>> Search([FromBody] MovieSearchFilterDto filter)
    {
        try
        {
            if (filter == null)
                return BadRequest(ApiResponse<List<MovieDto>>.FailResponse("Filter data is required."));

            var movies = await _movieService.GetAllByFilter(filter);

            if (movies == null || !movies.Any())
                return NotFound(ApiResponse<List<MovieDto>>.FailResponse("No movies found matching your search criteria."));

            return Ok(ApiResponse<List<MovieDto>>.SuccessResponse(movies.ToList(), "Movies retrieved successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<MovieDto>>.FailResponse(
                "An error occurred while searching for movies.", new List<string> { ex.Message }));
        }
    }

    // i need enpoint to get all movies and all tvshows 
    [HttpGet]
    [Route("all")]
    public async Task<ActionResult<ApiResponse<AllMediaDto>>> GetAllMedia()
    {
        try
        {
            var media = await _movieService.GetAllMediaAsync();
            return Ok(ApiResponse<AllMediaDto>.SuccessResponse(media));
        }
        catch (Exception ex)
        {
            return BadRequest(
                ApiResponse<AllMediaDto>.FailResponse(
                    "Failed to get media",
                    new List<string> { ex.Message }
                )
            );
        }
    }



    // i need endpoint to get all 12 random movies and tvshows 
    [HttpGet]
    [Route("random")]
    public async Task<ActionResult<ApiResponse<List<AllMediaDto>>>> GetRandomMedia()
    {
        try
        {
            var media = await _movieService.GetRandomMediaAsync(12);

            return Ok(ApiResponse<List<AllMediaDto>>.SuccessResponse(media, "Random media loaded successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(
                ApiResponse<List<AllMediaDto>>.FailResponse(
                    "Failed to get random media",
                    new List<string> { ex.Message }
                )
            );
        }
    }


















}

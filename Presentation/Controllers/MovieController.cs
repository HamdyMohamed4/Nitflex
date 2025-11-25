using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using ApplicationLayer.Services;
using Domains;
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
    public class MovieController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IUserService _userService;

        public MovieController(IMovieService movieService, IUserService userService)
        {
            _movieService = movieService;
            _userService = userService;
        }



        // GET: api/Movie/genre/{genreId}?page=1&pageSize=20
        [HttpGet("genre/{genreId:guid}")]
        public async Task<ActionResult<ApiResponse<GenreMoviesResponseDto>>> GetByGenre(Guid genreId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        { 
            try 
            {
                if (genreId == Guid.Empty) return
                        BadRequest(ApiResponse<GenreMoviesResponseDto>.FailResponse("Invalid genre id."));
                var result = await _movieService.GetMoviesByGenreAsync(genreId, page, pageSize);
                if (result == null || result.MediaData == null || result.MediaData.Count == 0)
                    return NotFound(ApiResponse<GenreMoviesResponseDto>.FailResponse("No movies found for the specified genre.")); 
                return Ok(ApiResponse<GenreMoviesResponseDto>.SuccessResponse(result, "Movies by genre retrieved successfully.")); } 
            catch (Exception ex) { return BadRequest(ApiResponse<GenreMoviesResponseDto>.FailResponse("Failed to retrieve movies by genre", new List<string> { ex.Message })); 
            }
        }

        // GET: api/Movie/genre/name/{genreName}?page=1&pageSize=20
        [HttpGet("genre/name/{genreName}")]
        public async Task<ActionResult<ApiResponse<GenreMoviesResponseDto>>> GetByGenreName(string genreName, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(genreName))
                    return BadRequest(ApiResponse<GenreMoviesResponseDto>.FailResponse("Genre name is required."));

                var result = await _movieService.GetMoviesByGenreNameAsync(genreName, page, pageSize);

                if (result == null || result.MediaData == null || result.MediaData.Count == 0)
                    return NotFound(ApiResponse<GenreMoviesResponseDto>.FailResponse("No movies found for this genre."));

                return Ok(ApiResponse<GenreMoviesResponseDto>.SuccessResponse(result, "Movies filtered by genre name retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<GenreMoviesResponseDto>.FailResponse("Error retrieving movies", new List<string> { ex.Message }));
            }
        }


        // GET: api/Movie/featured?limit=10
        [HttpGet("featured")]
        public async Task<ActionResult<ApiResponse<List<MovieDto>>>> GetFeatured([FromQuery] int limit = 10)
        {
            try
            {
                var movies = await _movieService.GetFeaturedAsync(limit);
                if (movies == null || !movies.Any())
                    return NotFound(ApiResponse<List<MovieDto>>.FailResponse("No featured movies found."));

                return Ok(ApiResponse<List<MovieDto>>.SuccessResponse(movies.ToList(), "Featured movies retrieved."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<MovieDto>>.FailResponse("Failed to retrieve featured movies", new List<string> { ex.Message }));
            }
        }

        //// GET: api/Movie/featured-trailers?limit=10
        //// Returns featured movies (lightweight) that include TrailerUrl for client preview/play trailer
        //[HttpGet("featured-trailers")]
        //public async Task<ActionResult<ApiResponse<List<MovieDto>>>> GetFeaturedWithTrailers([FromQuery] int limit = 10)
        //{
        //    try
        //    {
        //        // استدعاء الميثود من الـ Service للحصول على الأفلام المميزة مع التريلر
        //        var movies = await _movieService.GetFeaturedWithTrailersAsync(limit);

        //        if (movies == null || !movies.Any())
        //            return NotFound(ApiResponse<List<MovieDto>>.FailResponse("No featured movies found."));

        //        return Ok(ApiResponse<List<MovieDto>>.SuccessResponse(movies, "Featured movies with trailers retrieved."));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ApiResponse<List<MovieDto>>.FailResponse("Failed to retrieve featured trailers", new List<string> { ex.Message }));
        //    }
        //}


        // GET: api/Movie/{id}/trailer
        //[HttpGet("{id:guid}/trailer")]
        //public async Task<ActionResult<ApiResponse<string>>> GetTrailer(Guid id)
        //{
        //    try
        //    {
        //        if (id == Guid.Empty)
        //            return BadRequest(ApiResponse<string>.FailResponse("Invalid movie id."));

        //        // استدعاء الميثود المعدلة من السيرفيس
        //        var movie = await _movieService.GetTrailerByIdAsync(id);

        //        if (movie == null)
        //            return NotFound(ApiResponse<string>.FailResponse("Movie not found or not featured."));

        //        // تحقق من وجود رابط التريلر
        //        if (string.IsNullOrWhiteSpace(movie.TrailerUrl))
        //            return NotFound(ApiResponse<string>.FailResponse("Trailer not available for this movie."));

        //        // إرجاع رابط التريلر إذا كان متاحًا
        //        return Ok(ApiResponse<string>.SuccessResponse(movie.TrailerUrl, "Trailer URL retrieved."));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ApiResponse<string>.FailResponse("Failed to retrieve trailer URL", new List<string> { ex.Message }));
        //    }
        //}




        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<MovieDto>>> GetMovieById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<MovieDto>.FailResponse("Invalid movie id."));

                // استدعاء الميثود من السيرفيس للحصول على تفاصيل الفيلم
                var movie = await _movieService.GetById(id);

                if (movie == null)
                    return NotFound(ApiResponse<MovieDto>.FailResponse("Movie not found."));

                return Ok(ApiResponse<MovieDto>.SuccessResponse(movie, "Movie details retrieved."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<MovieDto>.FailResponse("Failed to retrieve movie details", new List<string> { ex.Message }));
            }
        }


        // POST: api/tmdbimport/import-all
        [HttpPost("import-all")]
        public async Task<IActionResult> ImportAll()
        {
            // 1. Import Genres
            await _movieService.ImportGenresAsync();

            // 2. Import Top Rated Movies
            await _movieService.ImportTopRatedMoviesAsync();
            await _movieService.ImportTopRatedShowsAsync();
            await _movieService.ImportAllMoviesAsync();
            await _movieService.ImportAllTVsAsync();

            // 3. Import Cast for each movie
            var allMovies = await _movieService.GetAllImportedMoviesAsync();
            foreach (var movie in allMovies)
            {
                await _movieService.ImportCastForMovieAsync(movie, movie.TmdbId); // لازم تضيف TmdbId للـ Movie
            }


            var allTvshows = await _movieService.GetAllImportedTvshowsAsync();
            foreach (var movie in allTvshows)
            {
                await _movieService.ImportCastForShowsAsync(movie, movie.TmdbId); // لازم تضيف TmdbId للـ Movie
            }



            return Ok("All data imported successfully!");
        }




        //[HttpPost("genres")]
        //public async Task<IActionResult> ImportGenres()
        //{
        //    await _movieService.ImportGenresAsync();
        //    return Ok("Genres imported.");
        //}

        //[HttpPost("movies/top-rated")]
        //public async Task<IActionResult> ImportTopRatedMovies()
        //{
        //    await _movieService.ImportTopRatedMoviesAsync();
        //    return Ok("Top rated movies imported.");
        //}


        //[HttpPost("import/popular")]
        //public async Task<IActionResult> ImportPopularMovies()
        //{
        //    var count = await _movieService.ImportPopularMoviesAsync();

        //    return Ok(new
        //    {
        //        Message = $"Imported {count} movies successfully!"
        //    });
        //}




        // GET: api/Movie/{id}/play
        // Returns a streaming locator for the movie. Requires authenticated user.
        //[HttpGet("{id:guid}/play")]
        ////[Authorize(Roles = "User")]
        //public async Task<ActionResult<ApiResponse<string>>> Play(Guid id)
        //{
        //    try
        //    {
        //        if (id == Guid.Empty)
        //            return BadRequest(ApiResponse<string>.FailResponse("Invalid movie id"));

        //        var profileId = _userService.GetLoggedInUser();
        //        var url = await _movieService.GetStreamingUrlAsync(id, profileId);

        //        if (string.IsNullOrWhiteSpace(url))
        //            return NotFound(ApiResponse<string>.FailResponse("Streaming URL not available for this movie."));

        //        return Ok(ApiResponse<string>.SuccessResponse(url, "Streaming URL retrieved."));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ApiResponse<string>.FailResponse("Failed to get streaming URL", new List<string> { ex.Message }));
        //    }
        //}

        //// GET: api/Movie/featured?limit=10
        //[HttpGet("featured")]
        //public async Task<ActionResult<ApiResponse<List<MovieDto>>>> GetFeatured([FromQuery] int limit = 10)
        //{
        //    try
        //    {
        //        var movies = await _movieService.GetFeaturedAsync(limit);
        //        if (movies == null || !movies.Any())
        //            return NotFound(ApiResponse<List<MovieDto>>.FailResponse("No featured movies found."));

        //        return Ok(ApiResponse<List<MovieDto>>.SuccessResponse(movies.ToList(), "Featured movies retrieved."));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ApiResponse<List<MovieDto>>.FailResponse("Failed to retrieve featured movies", new List<string> { ex.Message }));
        //    }
        //}
    }
}
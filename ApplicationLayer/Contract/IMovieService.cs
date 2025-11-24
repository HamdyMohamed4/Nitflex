using ApplicationLayer.Dtos;
using Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    /// <summary>
    /// Service contract for movie-related operations.
    /// Extends the generic IBaseService for basic CRUD and adds movie-specific APIs
    /// such as filtering, genre-based listings and playback URL retrieval.
    /// </summary>
    public interface IMovieService : IBaseService<Movie, MovieDto>
    {
        Task ImportGenresAsync();
        Task ImportTopRatedMoviesAsync();

        Task<List<Movie>> GetAllImportedMoviesAsync();

        Task ImportCastForMovieAsync(Movie movie, int tmdbMovieId);
        Task<AllMediaDto> GetMediaByGenreIdAsync(Guid genreId);
        Task<List<AllMediaDto>> GetRandomMediaAsync(int count);
        Task<GenreMoviesResponseDto> GetMoviesByGenreNameAsync(string genreName, int page = 1, int pageSize = 20);
        /// <summary>
        /// Get all movies optionally filtered by genre and/or search term.
        /// </summary>
        Task<IEnumerable<MovieDto>> GetAllAsync(Guid? genreId = null, string? search = null);

        /// <summary>
        /// Get movies using a rich filter object (search term, language, audio type, sort option, etc).
        /// </summary>
        Task<IEnumerable<MovieDto>> GetAllByFilter(MovieSearchFilterDto filter);

        /// <summary>
        /// Get movie details by id.
        /// </summary>
        Task<MovieDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Create a movie.
        /// </summary>
        Task<MovieDto> CreateAsync(MovieDto dto);

        /// <summary>
        /// Update a movie by id.
        /// </summary>
        Task<bool> UpdateAsync(Guid id, MovieDto dto);

        /// <summary>
        /// Delete a movie by id.
        /// </summary>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Get movies for a specific genre with paging metadata.
        /// Useful for category pages showing movies by genre.
        /// </summary>
        Task<GenreMoviesResponseDto> GetMoviesByGenreAsync(Guid genreId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get featured movies for the home/hero carousel.
        /// </summary>
        Task<IEnumerable<MovieDto>> GetFeaturedAsync(int limit = 10);

        /// <summary>
        /// Get a streaming/locator URL for a movie for the given profile (or null if not available).
        /// Service implementations can use profile/subscription info to return proper locators.
        /// </summary>
        Task<string?> GetStreamingUrlAsync(Guid movieId, Guid profileId);

        /// <summary>
        /// Get movies matching any of the supplied genre ids.
        /// </summary>
        Task<IEnumerable<MovieDto>> GetByGenreIdsAsync(IEnumerable<Guid> genreIds, int limit = 50);

        Task<List<MovieDto>> GetFeaturedWithTrailersAsync(int limit = 10);
        Task<MovieDto?> GetTrailerByIdAsync(Guid id);

        Task<AllMediaDto> GetAllMediaAsync();
    }
}

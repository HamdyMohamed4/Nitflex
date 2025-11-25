using ApplicationLayer.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    public interface ITmdbService
    {
        //Task<Dtos.TmdbMovieDetails?> GetMovieDetailsAsync(int tmdbMovieId);

        //Task<Dtos.TmdbMovieResponse?> GetPopularMoviesAsync(int page = 1);

        //Task<Dtos.TmdbTvResponse?> GetPopularTvShowsAsync(int page = 1);

        Task<TmdbCreditsResponse?> GetTvCastAsync(int tvId);
        Task<TmdbCreditsResponse?> GetMovieCastAsync(int movieId);
        Task<TmdbEpisodeVideosResponse?> GetEpisodeVideosAsync(int tvId, int seasonNumber, int episodeNumber);
        Task<TmdbTvResponse?> GetTopRatedShowsAsync();
        Task<TmdbVideosResponse?> GetTvVideosAsync(int tvId);
        Task<TmdbMovieResponse?> GetAllMoviesAsync();


        Task<TmdbTvDetails?> GetTvDetailsAsync(int tvId);

        Task<TmdbSeasonDetails?> GetSeasonDetailsAsync(int tvId, int seasonNumber);



        Task<TmdbTvResponse?> GetAllTvShowsAsync();

        Task<TmdbCreditsResponse?> GetTvCreditsAsync(int tvId);

        Task<TmdbMovieResponse?> GetTopRatedMoviesAsync();
        Task<TmdbVideosResponse?> GetMovieVideosAsync(int movieId);

        Task<TmdbCreditsResponse?> GetMovieCreditsAsync(int movieId);


        Task<TmdbGenreResponse?> GetGenresAsync(string type = "movie");


        Task<TmdbMovieResponse?> GetPopularTVShowsAsync();





    }
}

using ApplicationLayer.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    public interface ITmdbService
    {
        //Task<Dtos.TmdbMovieDetails?> GetMovieDetailsAsync(int tmdbMovieId);

        //Task<Dtos.TmdbMovieResponse?> GetPopularMoviesAsync(int page = 1);

        //Task<Dtos.TmdbTvResponse?> GetPopularTvShowsAsync(int page = 1);



        Task<TmdbMovieResponse?> GetTopRatedMoviesAsync();
        Task<TmdbVideosResponse?> GetMovieVideosAsync(int movieId);

        Task<TmdbCreditsResponse?> GetMovieCreditsAsync(int movieId);


        Task<TmdbGenreResponse?> GetGenresAsync(string type = "movie");


        Task<TmdbMovieResponse?> GetPopularTVShowsAsync();





    }
}

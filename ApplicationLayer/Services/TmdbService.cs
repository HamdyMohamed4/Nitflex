using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Azure;
using Domains;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

//public class TmdbService: ITmdbService
//{
//    private readonly HttpClient _httpClient;
//    private readonly string _apiKey = "5cb44a2467305747d7e9c95eb0b14217";
//    private readonly string _baseUrl = "https://api.themoviedb.org/3/";

//    public TmdbService()
//    {
//        _httpClient = new HttpClient();
//        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//    }

//    private async Task<T?> GetAsync<T>(string endpoint)
//    {
//        var response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}&api_key={_apiKey}");

//        if (!response.IsSuccessStatusCode)
//            return default;

//        var json = await response.Content.ReadAsStringAsync();
//        return JsonConvert.DeserializeObject<T>(json);
//    }

//    // Get popular Movies
//    public async Task<TmdbMovieResponse?> GetPopularMoviesAsync(int page = 1)
//    {
//        return await GetAsync<TmdbMovieResponse>($"movie/popular?page={page}");
//    }

//    // Get popular TV Shows
//    public async Task<TmdbTvResponse?> GetPopularTvShowsAsync(int page = 1)
//    {
//        return await GetAsync<TmdbTvResponse>($"tv/popular?page={page}");
//    }

//    // Get details for a movie
//    public async Task<TmdbMovieDetails?> GetMovieDetailsAsync(int movieId)
//    {
//        return await GetAsync<TmdbMovieDetails>($"movie/{movieId}");
//    }


//}



public class TmdbService: ITmdbService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey = "5cb44a2467305747d7e9c95eb0b14217";

    public TmdbService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    }

    public async Task<TmdbCreditsResponse?> GetTvCastAsync(int tvId)
    {
        var response = await _httpClient.GetAsync($"tv/{tvId}/credits?api_key={_apiKey}&language=en-US");

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<TmdbCreditsResponse>();
        return result;
    }
    public async Task<TmdbMovieResponse?> GetTopRatedMoviesAsync()
    {
        var response = await _httpClient.GetAsync($"movie/top_rated?api_key={_apiKey}&language=en-US&page=1");
        return await response.Content.ReadFromJsonAsync<TmdbMovieResponse>();
    }

    public async Task<TmdbTvResponse?> GetTopRatedShowsAsync()
    {
        var allShows = new List<TmdbTv>();

        for (int i = 1; i <= 3; i++)
        {
            var response = await _httpClient.GetAsync(
                $"tv/top_rated?api_key={_apiKey}&language=en-US&page={i}"
            );

            if (!response.IsSuccessStatusCode)
                continue;

            var result = await response.Content.ReadFromJsonAsync<TmdbTvResponse>();

            if (result?.results != null)
                allShows.AddRange(result.results);
        }

        return new TmdbTvResponse
        {
            results = allShows
        };

    }

    public async Task<TmdbMovieResponse?> GetAllMoviesAsync()
    {
        var allMovies = new List<TmdbMovie>();

        for (int i = 1; i <= 3; i++)
        {
            var response = await _httpClient.GetAsync(
                $"discover/movie?api_key={_apiKey}&include_adult=false&include_video=false&language=en-US&page={i}&sort_by=popularity.desc"
            );

            if (!response.IsSuccessStatusCode)
                continue;

            var result = await response.Content.ReadFromJsonAsync<TmdbMovieResponse>();

            if (result?.results != null)
                allMovies.AddRange(result.results);
        }

        return new TmdbMovieResponse
        {
            results = allMovies
        };
    }

    public async Task<TmdbCreditsResponse?> GetMovieCastAsync(int movieId)
    {
        try
        {

            var response = await _httpClient.GetAsync(
               $"movie/{movieId}/credits?api_key={_apiKey}&language=en-US"
           );

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TmdbCreditsResponse>(json);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in GetMovieCastAsync: {ex.Message}");
            return null;
        }
    }


    public async Task<TmdbTvResponse?> GetAllTvShowsAsync()
    {
        var allShows = new List<TmdbTv>();

        for (int i = 1; i <= 3; i++)
        {
            var response = await _httpClient.GetAsync(
                $"discover/tv?api_key={_apiKey}&include_adult=false&language=en-US&page={i}&sort_by=popularity.desc"
            );

            if (!response.IsSuccessStatusCode)
                continue;

            var result = await response.Content.ReadFromJsonAsync<TmdbTvResponse>();

            if (result?.results != null)
                allShows.AddRange(result.results);
        }

        return new TmdbTvResponse
        {
            results = allShows
        };
    }



    //discover/movie? include_adult = false & include_video = false & language = en - US & page = 1 & sort_by = popularity.desc


    public async Task<TmdbMovieResponse?> GetPopularTVShowsAsync()
    {
        var response = await _httpClient.GetAsync($"tv/popular?api_key={_apiKey}&language=en-US&page=1");
        return await response.Content.ReadFromJsonAsync<TmdbMovieResponse>();
    }

    public async Task<TmdbGenreResponse?> GetGenresAsync(string type = "movie")
    {
        var response = await _httpClient.GetAsync($"genre/{type}/list?api_key={_apiKey}&language=en-US");
        return await response.Content.ReadFromJsonAsync<TmdbGenreResponse>();
    }

    public async Task<TmdbCreditsResponse?> GetMovieCreditsAsync(int movieId)
    {
        var response = await _httpClient.GetAsync($"movie/{movieId}/credits?api_key={_apiKey}");
        return await response.Content.ReadFromJsonAsync<TmdbCreditsResponse>();
    }
    public async Task<TmdbCreditsResponse?> GetTvCreditsAsync(int tvId)
    {
        var response = await _httpClient.GetAsync($"tv/{tvId}/credits?api_key={_apiKey}");
        return await response.Content.ReadFromJsonAsync<TmdbCreditsResponse>();
    }

    public async Task<TmdbVideosResponse?> GetMovieVideosAsync(int movieId)
    {
        var response = await _httpClient.GetAsync($"movie/{movieId}/videos?api_key={_apiKey}&language=en-US");
        return await response.Content.ReadFromJsonAsync<TmdbVideosResponse>();
    }
    public async Task<TmdbVideosResponse?> GetTvVideosAsync(int tvId)
    {
        var response = await _httpClient.GetAsync($"tv/{tvId}/videos?api_key={_apiKey}&language=en-US");
        return await response.Content.ReadFromJsonAsync<TmdbVideosResponse>();
    }


    public async Task<TmdbTvDetails?> GetTvDetailsAsync(int tvId)
    {
        var response = await _httpClient.GetAsync($"tv/{tvId}?api_key={_apiKey}&language=en-US");
        return await response.Content.ReadFromJsonAsync<TmdbTvDetails>();
    }

    public async Task<TmdbSeasonDetails?> GetSeasonDetailsAsync(int tvId, int seasonNumber)
    {
        var response = await _httpClient.GetAsync($"tv/{tvId}/season/{seasonNumber}?api_key={_apiKey}&language=en-US");
        return await response.Content.ReadFromJsonAsync<TmdbSeasonDetails>();
    }

    public async Task<TmdbEpisodeVideosResponse?> GetEpisodeVideosAsync(int tvId, int seasonNumber, int episodeNumber)
    {
        var response = await _httpClient.GetAsync(
            $"tv/{tvId}/season/{seasonNumber}/episode/{episodeNumber}/videos?api_key={_apiKey}&language=en-US"
        );

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<TmdbEpisodeVideosResponse>();
    }







}

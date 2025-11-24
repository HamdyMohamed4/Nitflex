using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Domains;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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
    private readonly string _apiKey = "YOUR_TMDB_API_KEY";

    public TmdbService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    }

    public async Task<TmdbMovieResponse?> GetTopRatedMoviesAsync()
    {
        var response = await _httpClient.GetAsync($"movie/top_rated?api_key={_apiKey}&language=en-US&page=1");
        return await response.Content.ReadFromJsonAsync<TmdbMovieResponse>();
    }

    public async Task<TmdbMovieResponse?> GetPopularTVShowsAsync()
    {
        var response = await _httpClient.GetAsync($"tv/popular?api_key={_apiKey}&language=en-US&page=1");
        return await response.Content.ReadFromJsonAsync<TmdbMovieResponse>();
    }

    public async Task<TmdbGenreResponse?> GetGenresAsync(string type = "movie")
    {
        var response = await _httpClient.GetAsync($"{type}/list?api_key={_apiKey}&language=en-US");
        return await response.Content.ReadFromJsonAsync<TmdbGenreResponse>();
    }

    public async Task<TmdbCreditsResponse?> GetMovieCreditsAsync(int movieId)
    {
        var response = await _httpClient.GetAsync($"movie/{movieId}/credits?api_key={_apiKey}");
        return await response.Content.ReadFromJsonAsync<TmdbCreditsResponse>();
    }

    public async Task<TmdbVideosResponse?> GetMovieVideosAsync(int movieId)
    {
        var response = await _httpClient.GetAsync($"movie/{movieId}/videos?api_key={_apiKey}&language=en-US");
        return await response.Content.ReadFromJsonAsync<TmdbVideosResponse>();
    }

}

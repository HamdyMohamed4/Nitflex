using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class TmdbMovie
    {
        public int id { get; set; }
        public string title { get; set; } = string.Empty;
        public string overview { get; set; } = string.Empty;
        public string? poster_path { get; set; }
        public string? backdrop_path { get; set; }
        public string? original_language { get; set; }

        
        public string release_date { get; set; } = string.Empty;
        public List<int>? genre_ids { get; set; }
    }

    public class TmdbMovieResponse
    {
        public List<TmdbMovie> results { get; set; } = new();
    }

    public class TmdbGenre
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
    }

    public class TmdbGenreResponse
    {
        public List<TmdbGenre> genres { get; set; } = new();
    }

    public class TmdbCastMember
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public string? profile_path { get; set; }
        public string character { get; set; } = string.Empty;
    }

    public class TmdbCreditsResponse
    {
        public List<TmdbCastMember> cast { get; set; } = new();
    }

    public class TmdbVideo
    {
        public string key { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty; // Trailer, Clip
        public string site { get; set; } = string.Empty; // YouTube
    }

    public class TmdbVideosResponse
    {
        public List<TmdbVideo> results { get; set; } = new();
    }







}

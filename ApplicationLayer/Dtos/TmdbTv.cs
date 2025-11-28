using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class TmdbTv
    {
        public string? backdrop_path { get; set; }
        public string? first_air_date { get; set; }
        public string? title { get; set; }
        public List<int>? genre_ids { get; set; }
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public List<string>? origin_country { get; set; }
        public string original_language { get; set; } = string.Empty;
        public string original_name { get; set; } = string.Empty;
        public string overview { get; set; } = string.Empty;
        public double popularity { get; set; }
        public string? poster_path { get; set; }
        public double vote_average { get; set; }
        public int vote_count { get; set; }
        public int release_date { get; set; }

       
    }
}

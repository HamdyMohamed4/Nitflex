using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class TmdbTvDetails
    {
        public int id { get; set; }
        public string name { get; set; }
        public string original_name { get; set; }
        public string overview { get; set; }
        public string first_air_date { get; set; }
        public string poster_path { get; set; }
        public string original_language { get; set; }

        public List<TmdbGenre> genres { get; set; } = new();

        public List<TmdbSeason> seasons { get; set; } = new();
    }

    public class TmdbSeason
    {
        public string air_date { get; set; }
        public int episode_count { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string overview { get; set; }
        public string poster_path { get; set; }
        public int season_number { get; set; }
    }

}

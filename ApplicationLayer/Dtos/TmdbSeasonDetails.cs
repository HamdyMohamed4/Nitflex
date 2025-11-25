using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class TmdbSeasonDetails
    {
        public string _id { get; set; }
        public string air_date { get; set; }
        public List<TmdbEpisode> episodes { get; set; } = new();
        public string name { get; set; }
        public string overview { get; set; }
        public int id { get; set; }
        public string poster_path { get; set; }
        public int season_number { get; set; }
    }

    public class TmdbEpisode
    {
        public int id { get; set; }
        public string name { get; set; }
        public string overview { get; set; }
        public int episode_number { get; set; }
        public int? runtime { get; set; }
        public string still_path { get; set; }
    }

}

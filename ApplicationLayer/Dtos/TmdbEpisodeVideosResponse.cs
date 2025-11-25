using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class TmdbEpisodeVideosResponse
    {
        public int id { get; set; }
        public List<TmdbVideo> results { get; set; } = new();
    }

 

}

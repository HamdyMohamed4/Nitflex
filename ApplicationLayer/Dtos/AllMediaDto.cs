using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class AllMediaDto
    {
        public List<MovieDto> Movies { get; set; }
        public List<TvShowDto> TvShows { get; set; }
    }
}

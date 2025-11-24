using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class TmdbTvResponse
    {
        public int page { get; set; }
        public List<TmdbTv> results { get; set; }
    }
}

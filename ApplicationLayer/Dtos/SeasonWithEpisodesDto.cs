using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class SeasonWithEpisodesDto
    {
        public int SeasonNumber { get; set; }
        public List<EpisodeDto> Episodes { get; set; } = new();
    }

}

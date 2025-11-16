using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class CreateEpisodeDto:BaseDto
    {
        public string Title { get; set; } = null!;
        public int EpisodeNumber { get; set; }
        public int Duration { get; set; }
        public string? VideoUrl { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class EpisodeDto:BaseDto
    {
        public string Title { get; set; } = null!;
        public int EpisodeNumber { get; set; }
        public int Duration { get; set; }
        public string? VideoUrl { get; set; }

        public Guid SeasonId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class MovieDto:BaseDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int ReleaseYear { get; set; }
        public int Duration { get; set; }
        public string? PosterUrl { get; set; }
        public bool IsFeatured { get; set; }
        public string? Language { get; set; }
        public string? AudioType { get; set; }  // Original / Dubbed / Subtitled

    }
}

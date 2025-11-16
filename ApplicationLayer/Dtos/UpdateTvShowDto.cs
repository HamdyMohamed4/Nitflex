using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class UpdateTvShowDto:BaseDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public Guid? GenreId { get; set; }
        public string? PosterUrl { get; set; }
        public string? BannerUrl { get; set; }
    }
}

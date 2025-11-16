using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class TVShow : BaseTable
    {
        [MaxLength(250)]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public AgeRating AgeRating { get; set; }

        [MaxLength(500)]
        public string PosterUrl { get; set; } = string.Empty;
        [MaxLength(500)]
        public string BannerUrl { get; set; } = string.Empty;

        public ICollection<Season> Seasons { get; set; } = new List<Season>();
        public ICollection<TVShowGenre> TVShowGenres { get; set; } = new List<TVShowGenre>();
        public ICollection<TvShowCast> Castings { get; set; } = new List<TvShowCast>();
    }

}

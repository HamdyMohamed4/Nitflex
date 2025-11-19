using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class Movie : BaseTable
    {
        [MaxLength(250)]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public int DurationMinutes { get; set; }
        public AgeRating AgeRating { get; set; }

        [MaxLength(500)]
        public string? Language { get; set; }  // English / Arabic / Spanish / French
        public string? AudioType { get; set; }  // Original / Dubbed / Subtitled


        [MaxLength(500)]
        public string PosterUrl { get; set; } = string.Empty;
        [MaxLength(500)]
        public string BannerUrl { get; set; } = string.Empty;
        [MaxLength(500)]
        public string TrailerUrl { get; set; } = string.Empty;

        public bool IsFeatured { get; set; } = false;
        
        public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
        public ICollection<MovieCast> Castings { get; set; } = new List<MovieCast>();
    }

}

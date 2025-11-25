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
        public string? Name { get; set; } = string.Empty;
        public string? Title { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public int? ReleaseYear { get; set; }
        public int? DurationMinutes { get; set; }
        public int TmdbId { get; set; }
        public AgeRating AgeRating { get; set; }

        [MaxLength(500)]
        public string? Language { get; set; }  // English / Arabic / Spanish / French
        public string? AudioType { get; set; }  // Original / Dubbed / Subtitled

        [MaxLength(500)]
        public string? PosterUrl { get; set; } = string.Empty;
        public bool IsFeatured { get; set; } = false;
        public MediaType? Type { get; set; } = MediaType.Movie;

        public ICollection<Season> Seasons { get; set; } = new List<Season>();
        public ICollection<TVShowGenre> TVShowGenres { get; set; } = new List<TVShowGenre>();
        public ICollection<TvShowCast> Castings { get; set; } = new List<TvShowCast>();
    }

}

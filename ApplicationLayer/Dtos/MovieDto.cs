using Domains;
using System;
using System.Collections.Generic;

namespace ApplicationLayer.Dtos
{
    public class MovieDto : BaseDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? ReleaseYear { get; set; }
        public int? DurationMinutes { get; set; }
        public AgeRating AgeRating { get; set; }
        public string? PosterUrl { get; set; }
        public int TmdbId { get; set; }
        public string? VideoUrl { get; set; }
        public string? TrailerUrl { get; set; }
        public bool IsFeatured { get; set; }
        public MediaType? Type { get; set; } = MediaType.Movie;
        public string? Language { get; set; }
        public string? AudioType { get; set; }

        public List<Guid> GenreIds { get; set; } = new();
        public List<GenreDto> GenresNames { get; set; } = new();
        public List<CastDto> Castings { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;
using Domains;

namespace ApplicationLayer.Dtos
{
    public class MovieDto : BaseDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int ReleaseYear { get; set; }

        // Match domain property name for smoother AutoMapper mapping
        public int DurationMinutes { get; set; }

        public AgeRating AgeRating { get; set; }

        public string? PosterUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string? TrailerUrl { get; set; }

        public bool IsFeatured { get; set; }
        public string? Language { get; set; }
        public string? AudioType { get; set; }  // Original / Dubbed / Subtitled

        // Genres: both IDs (for simple client filtering) and detailed DTOs (for UI)
        public List<Guid> GenreIds { get; set; } = new();
        public List<GenreDto> Genres { get; set; } = new();

        // URL/locator used by the player when user clicks Play
        public string? StreamingUrl { get; set; }

        // Lightweight cast listing (names). Expand to full cast DTOs if needed.
        public List<string> Cast { get; set; } = new();
    }
}

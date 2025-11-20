using System;
using System.Collections.Generic;
using Domains;

namespace ApplicationLayer.Dtos
{
    public class TvShowDto : BaseDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        // Basic artwork
        public string? PosterUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string? TrailerUrl { get; set; }

        // Content metadata
        public AgeRating AgeRating { get; set; }
        public int? ReleaseYear { get; set; }
        public bool IsFeatured { get; set; }

        // Genre information (IDs for lightweight filters + full DTOs for UI)
        public Guid? PrimaryGenreId { get; set; }
        public List<Guid> GenreIds { get; set; } = new();
        public List<GenreDto> Genres { get; set; } = new();

        // Aggregated show data (useful for listing pages)
        public int NumberOfSeasons { get; set; }
        public int NumberOfEpisodes { get; set; }

        // Streaming locator exposed to the client when user clicks Play
        public string? StreamingUrl { get; set; }

        // Lightweight cast / creators for UI lists
        public List<string> Cast { get; set; } = new();

        // Optional: whether the show has been marked complete / ended
        public bool IsEnded { get; set; }
    }
}

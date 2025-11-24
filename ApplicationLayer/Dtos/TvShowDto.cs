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

        public string? Language { get; set; }
        public string? Type { get; set; }

        public List<GenreDto> Genres { get; set; } = new();

        public List<Guid> GenreIds { get; set; } = new();

        // Streaming locator exposed to the client when user clicks Play
        public string? StreamingUrl { get; set; }

        // Lightweight cast / creators for UI lists
        public List<string> Cast { get; set; } = new();

        // Optional: whether the show has been marked complete / ended
        public bool IsEnded { get; set; }


        // ⬇️ بديل NumberOfEpisodes و NumberOfSeasons
        public List<SeasonWithEpisodesDto> Seasons { get; set; } = new();
    }
}

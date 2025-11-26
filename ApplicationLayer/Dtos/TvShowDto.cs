using Domains;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApplicationLayer.Dtos
{
    public class TvShowDto : BaseDto
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




        public List<GenreDto> GenresNames { get; set; } = new();

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

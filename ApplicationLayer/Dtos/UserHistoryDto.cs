using Domains;
using System;

namespace ApplicationLayer.Dtos
{
public class UserHistoryDto : BaseDto
{
        // The user who watched the content
    public Guid ProfileId { get; set; }

        // The id of the content (movie, episode, etc.)
        public Guid ContentId { get; set; }

        // Type of the content (e.g. "Movie", "Episode", "Serie")
    public ContentType ContentType { get; set; }

        // Human readable title for the UI
        public string Title { get; set; } = string.Empty;

        // Current playback position
        public TimeSpan Position { get; set; }

        // Total duration of the content
        public TimeSpan Duration { get; set; }

        // Optional: season / episode metadata for series
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }

        // Optional: thumbnail or poster url to show in "Continue Watching" card
        public string? ThumbnailUrl { get; set; }

        // When the user last watched this content
        public DateTime LastWatchedAt { get; set; } = DateTime.UtcNow;

        // Computed convenience property for UI (0-100)
        public double ProgressPercent
        {
            get
            {
                if (Duration.TotalSeconds <= 0) return 0;
                return Math.Round((Position.TotalSeconds / Duration.TotalSeconds) * 100.0, 2);
            }
        }
    }
}
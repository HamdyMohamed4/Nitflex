using System;
using System.Collections.Generic;

namespace ApplicationLayer.Dtos
{
    public class GenreMoviesResponseDto
    {
        public Guid GenreId { get; set; }
        public string GenreName { get; set; } = string.Empty;

        // Movies belonging to this genre (page / preview)
        public List<MovieDto> MediaData { get; set; } = new();

        // Pagination / metadata
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Convenience computed property
        public bool HasMore => Page * PageSize < TotalCount;
    }
}

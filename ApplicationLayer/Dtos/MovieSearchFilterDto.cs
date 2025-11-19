using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class MovieSearchFilterDto : BaseDto
    {
        public string? SearchTerm { get; set; }    // Search Title, Description, Actor, Genre
        public string? Language { get; set; }      // English / Arabic / Spanish
        public string? AudioType { get; set; }     // Original / Dubbed / Subtitled
        public string? SortOption { get; set; }    // "Suggestion", "YearRelease", "A-Z", "Z-A"
    }
}

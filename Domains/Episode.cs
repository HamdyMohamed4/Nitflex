using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class Episode : BaseTable
    {
        [MaxLength(250)]
        public string Title { get; set; } = string.Empty;
        public int EpisodeNumber { get; set; }
        public int DurationMinutes { get; set; }
        public string VideoUrl { get; set; } = string.Empty;

        // Foreign Key (Changed from int to Guid)
        public Guid SeasonId { get; set; }
        public Season Season { get; set; } = default!;
    }

}

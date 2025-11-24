using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class Genre : BaseTable
    {
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int TmdbId { get; set; } // ده اللي كان ناقص

        public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
        public ICollection<TVShowGenre> TVShowGenres { get; set; } = new List<TVShowGenre>();
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class TVShowGenre
    {
        public Guid TVShowId { get; set; }
        public TVShow TVShow { get; set; } = default!;

        public Guid GenreId { get; set; }
        public Genre Genre { get; set; } = default!;
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class Season : BaseTable
    {
        public int SeasonNumber { get; set; }

        // Foreign Key (Changed from int to Guid)
        public Guid TvShowId { get; set; }
        public TVShow TvShow { get; set; } = default!;

        public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class TvShowCast
    {
        public Guid TvShowId { get; set; }
        public TVShow TvShow { get; set; } = default!;

        public Guid CastMemberId { get; set; }
        public CastMember CastMember { get; set; } = default!;

        public string CharacterName { get; set; } = string.Empty;
    }

}

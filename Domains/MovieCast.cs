using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class MovieCast:BaseTable
    {
        public Guid MovieId { get; set; }
        public Movie Movie { get; set; } = default!;

        public Guid CastMemberId { get; set; }
        public CastMember CastMember { get; set; } = default!;

        public string? CharacterName { get; set; } = string.Empty;
    }




}

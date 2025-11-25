using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class CastMember : BaseTable
    {
        [MaxLength(250)]
        public string? Name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? PhotoUrl { get; set; } = string.Empty;
        public string? Bio { get; set; } = string.Empty;

        public int TmdbId { get; set; } // ده اللي كان ناقص
    }

}

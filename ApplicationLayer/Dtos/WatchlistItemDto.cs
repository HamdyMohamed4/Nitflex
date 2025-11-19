using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class WatchlistItemDto : BaseDto
    {
        public Guid ProfileId { get; set; }
        public ContentType ContentType { get; set; }
        public Guid ContentId { get; set; }
        public DateTime AddedAt { get; set; }
    }
}

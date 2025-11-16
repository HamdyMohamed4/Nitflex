using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class AddToWatchlistDto:BaseDto
    {
        public ContentType ContentType { get; set; }
        public Guid ContentId { get; set; }
    }
}

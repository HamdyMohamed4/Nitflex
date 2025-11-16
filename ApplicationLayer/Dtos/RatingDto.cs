using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class RatingDto:BaseDto
    {
        public Guid UserId { get; set; }
        public ContentType ContentType { get; set; }
        public Guid ContentId { get; set; }
        public int RatingValue { get; set; }
        public DateTime RatedAt { get; set; }
    }
}

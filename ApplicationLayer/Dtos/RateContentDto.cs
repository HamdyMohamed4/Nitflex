using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class RateContentDto:BaseDto
    {
        public ContentType ContentType { get; set; }
        public Guid ContentId { get; set; }
        public int RatingValue { get; set; }
    }
}

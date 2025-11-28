using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class TicketDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<TicketMessageDto> Messages { get; set; } = new();
    }


}

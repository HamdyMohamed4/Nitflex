using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class Ticket : BaseTable
    {
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public string UserName { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public TicketStatus Status { get; set; } = TicketStatus.New;

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
    }

}

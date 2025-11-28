using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class TicketMessage : BaseTable
    {
        public Guid Id { get; set; }

        [Required]
        public Guid TicketId { get; set; }

        [Required]
        public Guid SenderId { get; set; } // User or Admin

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; }
        public bool IsFromAdmin { get; set; }
        [Required]
        public DateTime SentDate { get; set; }
      
    }

}

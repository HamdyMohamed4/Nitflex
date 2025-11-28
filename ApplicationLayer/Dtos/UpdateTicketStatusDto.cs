using Domains;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class UpdateTicketStatusDto
    {
        [Required(ErrorMessage = "The Status is required ")]
        [MaxLength(20)]
        public TicketStatus Status { get; set; }
    }

}

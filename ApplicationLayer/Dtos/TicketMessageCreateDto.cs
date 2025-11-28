using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class TicketMessageCreateDto
    {
        [Required(ErrorMessage = "The message is required ")]
        [MaxLength(2000)]
        public string Message { get; set; }
    }

}

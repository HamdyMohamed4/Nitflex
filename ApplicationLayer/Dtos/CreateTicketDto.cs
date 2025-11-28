using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class CreateTicketDto
    {

        [Required(ErrorMessage ="The title is required")]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "The message is required ")]
        [MaxLength(2000)]
        public string MessageContent { get; set; }
    }

}

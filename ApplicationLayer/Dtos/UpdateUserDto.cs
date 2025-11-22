using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class UpdateUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(50)]
        public string FullName { get; set; }

        public string? Role { get; set; }
    }

}

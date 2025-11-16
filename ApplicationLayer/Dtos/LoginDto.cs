using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class LoginDto
    {
        [Required]
        [EmailAddress ( ErrorMessage ="is not a valid email address.")]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }

        public LoginDto(string email, string password)
        {
            Email = email;
            Password = password;
        }

        public LoginDto() { }
    }
}

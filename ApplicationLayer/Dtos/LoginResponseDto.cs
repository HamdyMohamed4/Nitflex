using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string UserId { get; set; }
        public string Email { get; set; }
        public string RefreshToken { get; set; }
        public UserAccountStatus AccountStatus { get; set; } = UserAccountStatus.Pending;


        public LoginResponseDto() { }

        public LoginResponseDto(string token,string refreshToken, string userId)
        {
            AccessToken = token;
            RefreshToken = refreshToken;
            UserId = userId;
        }
    }
}

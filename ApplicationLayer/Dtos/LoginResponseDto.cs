using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public Guid UserId { get; set; }

        public LoginResponseDto() { }

        public LoginResponseDto(string token, string refreshToken, Guid userId)
        {
            Token = token;
            RefreshToken = refreshToken;
            UserId = userId;
        }
    }
}

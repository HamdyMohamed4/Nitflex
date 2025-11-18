using ApplicationLayer.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{

    public interface IAuthService
    {
        // 1. Sends the link (returns boolean success)
        Task<bool> SendMagicLinkAsync(string email);


        // 2. Confirms link and returns the tokens (The result of login)
        Task<LoginResponseDto?> RegisterUserFromMagicLinkAsync(string userId, string token);

        // 3. Registers with password and returns the tokens (The result of login)
        Task<LoginResponseDto?> RegisterWithPasswordAsync(RegisterDto model);

        // 4. Standard login
        Task<LoginResponseDto?> LoginAsync(string email, string password);

        Task<(Claim[] Claims, RegisterDto User)> GetUserWithRoles(string email);

    }
}



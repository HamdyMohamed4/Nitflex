using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationLayer.Dtos;
namespace ApplicationLayer.Contract
{
    public interface IUserService
    {
        Task<UserResultDto> RegisterAsync(RegisterDto registerDto);
        Task<UserResultDto> LoginAsync(LoginDto loginDto);
        Task LogoutAsync();
        Task<RegisterDto> GetUserByIdAsync(string userId);
        Task<RegisterDto> GetUserByEmailAsync(string email);
        Task<IEnumerable<RegisterDto>> GetAllUsersAsync();
        Guid GetLoggedInUser();
    }
}

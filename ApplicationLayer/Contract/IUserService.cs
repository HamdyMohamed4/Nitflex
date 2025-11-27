using ApplicationLayer.Dtos;
using InfrastructureLayer.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        Task<(UserResultDto Result, string? Token)> CreateUserWithoutPasswordAndGetTokenAsync(string email);
        Task<UserResultDto> ConfirmEmailAsync(Guid userId, string token);
        Task<UserResultDto> SetPasswordAsync(Guid userId, string password);
        Task<ApplicationUser?> GetUserByIdentityAsync(string userId);
        Task<ApplicationUser?> GetUserByIdWithProfilesAsync(string userId);
        Task<ApplicationUser?> GetUserByIdWithProfilesWithWatchListAsync(string userId);
        Task<ApplicationUser?> GetUserByIdWithProfilesWithHistoriesAsync(string userId);
        Task<LoginResponseDto> LoginWithOtpAsync(string email, string code);

        Task<IList<string>> GetUserRolesAsync(string userId);

        Task<ApplicationUser?> GetUserByEmailAsyncs(string email);

        Task<LoginResponseDto?> ConfirmSignUpAsync(string email, string token);
    }
}

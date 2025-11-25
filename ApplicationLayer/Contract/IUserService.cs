using ApplicationLayer.Dtos;
using InfrastructureLayer.UserModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    public interface IUserService
    {
        // New: transfer profile data (sourceUserId must match caller)
        Task<(bool Success, string Message)> TransferProfileAsync(Guid sourceUserId, Guid targetUserId);

        // Transfer profile by profileId -> target email (initiate / immediate depending on implementation)
        Task<(bool Success, string Message)> TransferProfileByEmailAsync(Guid sourceUserId, Guid profileId, string targetEmail);

        Task<UserResultDto> RegisterAsync(RegisterDto registerDto);
        Task<UserResultDto> LoginAsync(LoginDto loginDto);
        Task LogoutAsync();

        Task<(bool IsValid, string ErrorMessage, ApplicationUser? User)> ValidateLoginAsync(string email, string password);
        Task<RegisterDto> GetUserByIdAsync(string userId);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<IEnumerable<RegisterDto>> GetAllUsersAsync();
        Guid GetLoggedInUser();

        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
        Task<(UserResultDto Result, string? Token)> CreateUserWithoutPasswordAndGetTokenAsync(string email);
        Task<UserResultDto> ConfirmEmailAsync(Guid userId, string token);
        Task<UserResultDto> SetPasswordAsync(Guid userId, string password);
        Task<ApplicationUser?> GetUserByIdentityAsync(string userId);
        Task<ApplicationUser?> GetUserByIdWithProfilesAsync(string userId);
        Task<ApplicationUser?> GetUserByIdWithProfilesWithHistoriesAsync(string userId);
        Task<LoginResponseDto> LoginWithOtpAsync(string email, string code);
        Task<IList<string>> GetUserRolesAsync(string userId);
        Task<LoginResponseDto?> ConfirmSignUpAsync(string email, string token);
        Task<LoginDto?> GetUserByLoginAsync(string email);
    }
}

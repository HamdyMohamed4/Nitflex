using ApplicationLayer.Dtos;
using InfrastructureLayer.UserModels;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    public interface IAuthService
    {
        // 1. Send Magic Link
        Task<bool> SendMagicLinkAsync(string email);

        // 2. Register with Password + Tokens
        Task<LoginResponseDto?> RegisterAndGenerateTokensAsync(RegisterDto dto);

        // 3. Login with Password + Tokens
        Task<LoginResponseDto?> LoginAndGenerateTokensAsync(string email, string password);

        // 4. Confirm Email + Tokens
        Task<LoginResponseDto?> ConfirmEmailAndGenerateTokensAsync(string userId, string token);

        // 5. OTP: Generate + Send
        Task<bool> GenerateAndSendOtpAsync(string email);

        // 6. Login with OTP + Tokens
        Task<LoginResponseDto?> LoginWithOtpAndGenerateTokensAsync(string email, string code);

        // 7. Refresh Access Token
        Task<LoginResponseDto?> RefreshAccessTokenAsync(string refreshToken);

        Task<(Claim[] Claims, RegisterDto User)> GetUserWithRolesAsync(string email);

        Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword);


        Task<string?> GeneratePasswordResetTokenAsync(string email);

        Task SendPasswordResetEmailAsync(string email, string token);


        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);



        //// Magic link (email)
        //Task<bool> SendMagicLinkAsync(string email);

        //// Confirm magic link and return tokens (email+token passed in body)
        //Task<LoginResponseDto?> ConfirmEmailAndGenerateTokensAsync(string userIdentifier, string token);

        //// Register with email+password and return tokens
        //Task<LoginResponseDto?> RegisterAndGenerateTokensAsync(RegisterDto dto);

        //// Login with email+password and return tokens
        //Task<LoginResponseDto?> LoginAndGenerateTokensAsync(string email, string password);

        //// OTP flows
        //Task<bool> GenerateAndSendOtpAsync(string email); // generate 4-digit and send
        //Task<LoginResponseDto?> LoginWithOtpAndGenerateTokensAsync(string email, string code);

        //// Refresh access token using refresh token string
        //Task<LoginResponseDto?> RefreshAccessTokenAsync(string refreshToken);

        // Optional: expose helper to get claims+user (if needed by controllers)
    }
}

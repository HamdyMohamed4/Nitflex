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
    }
}

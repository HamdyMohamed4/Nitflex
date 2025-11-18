using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using InfrastructureLayer.UserModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Presentation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace ApplicationLayer.Services
{
    public class AuthService : IAuthService
    {
        //private readonly IUserService _userService;

        //public AuthService(IUserService userService)
        //{
        //    _userService = userService;
        //}

        //public async Task<bool> SendMagicLinkAsync(string email)
        //{
        //    var (result, token) = await _userService.CreateUserWithoutPasswordAndGetTokenAsync(email);

        //    if (!result.Success || token == null) return false;

        //    var magicLink = $"[Frontend_Base_URL]/auth/confirm-signup?userId={result.Id}&token={Uri.EscapeDataString(token)}";
        //    Console.WriteLine($"[DEMO] Magic Link for {email}: {magicLink}");

        //    return true;
        //}

        //public async Task<LoginResponseDto?> RegisterUserFromMagicLinkAsync(string userId, string token)
        //{
        //    if (!Guid.TryParse(userId, out Guid userGuid)) return null;

        //    var confirmResult = await _userService.ConfirmEmailAsync(userGuid, token);
        //    if (!confirmResult.Success) return null;

        //    var user = await _userService.GetUserByIdentityAsync(userId);
        //    if (user == null) return null;

        //    // --- TOKEN GENERATION ---
        //    return new LoginResponseDto("DEMO_JWT_TOKEN", "DEMO_REFRESH_TOKEN", user.Id);
        //}

        //public async Task<LoginResponseDto?> RegisterWithPasswordAsync(RegisterDto model)
        //{
        //    var registerResult = await _userService.RegisterAsync(model);

        //    if (!registerResult.Success) return null;

        //    return await LoginAsync(model.Email, model.Password);
        //}

        //public async Task<LoginResponseDto?> LoginAsync(string email, string password)
        //{
        //    var loginDto = new LoginDto(email, password);
        //    var loginResult = await _userService.LoginAsync(loginDto);

        //    if (!loginResult.Success) return null;

        //    var userDto = await _userService.GetUserByEmailAsync(email);
        //    if (userDto == null) return null;

        //    // --- TOKEN GENERATION ---
        //    return new LoginResponseDto("DEMO_JWT_TOKEN", "DEMO_REFRESH_TOKEN", userDto.Id);
        //}
        private readonly IRefreshTokens _refreshTokenService;
        private readonly TokenService _tokenService;
        private readonly IUserService _userService;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;

        public AuthService(IUserService userService,
            EmailService emailService ,
            TokenService tokenService,
            IRefreshTokens refreshTokenService,
            IConfiguration config)
        {
            _userService = userService;
            _emailService = emailService;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _config = config;
        }

        public async Task<bool> SendMagicLinkAsync(string email)
        {
            var (result, token) = await _userService.CreateUserWithoutPasswordAndGetTokenAsync(email);

            if (!result.Success || token == null) return false;

            var frontendUrl = _config["Frontend:BaseUrl"]; // <-- خد URL من settings

            var magicLink = $"{frontendUrl}?token={Uri.EscapeDataString(token)}";

            string emailBody = $@"
    <h3>Welcome to WatchMe!</h3>
    <p>Click the link below to complete your registration:</p>
    <a href='{magicLink}'>Confirm Registration</a>
    <p>If you did not request this, ignore this email.</p>";

            await _emailService.SendEmailAsync(email, "WatchMe Sign-Up Confirmation", emailBody);

            return true;
        }





        // باقي الـ AuthService بدون تغيير


        //public async Task<LoginResponseDto?> RegisterUserFromMagicLinkAsync(string userId, string token)
        //{
        //    if (!Guid.TryParse(userId, out Guid userGuid)) return null;

        //    var confirmResult = await _userService.ConfirmEmailAsync(userGuid, token);
        //    if (!confirmResult.Success) return null;

        //    var user = await _userService.GetUserByIdentityAsync(userId);
        //    if (user == null) return null;

        //    // TODO: Generate real JWT here
        //    return new LoginResponseDto("DEMO_JWT_TOKEN", "DEMO_REFRESH_TOKEN", user.Id.ToString());
        //}


        //    public async Task<LoginResponseDto?> RegisterUserFromMagicLinkAsync(string userId, string token)
        //    {
        //        if (!Guid.TryParse(userId, out Guid userGuid))
        //            return null;

        //        var confirmResult = await _userService.ConfirmEmailAsync(userGuid, token);
        //        if (!confirmResult.Success)
        //            return null;

        //        var user = await _userService.GetUserByIdentityAsync(userId);
        //        if (user == null)
        //            return null;

        //        // 👇 اجلب الـ Roles من الـ userService
        //        var roles = await _userService.GetUserRolesAsync(user.Id.ToString());

        //        // 👇 جهز الـ claims لو محتاج تستخدمهم للتوكن
        //        var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        //    new Claim(ClaimTypes.Email, user.Email)
        //};

        //        foreach (var role in roles)
        //            claims.Add(new Claim(ClaimTypes.Role, role));

        //        // 👇 توليد AccessToken و RefreshToken حقيقي (هنا مجرد Demo)
        //        var accessToken = "DEMO_JWT_TOKEN"; // لاحقًا استخدم الـ TokenService
        //        var refreshToken = "DEMO_REFRESH_TOKEN";

        //        return new LoginResponseDto
        //        {
        //            AccessToken = accessToken,
        //            RefreshToken = refreshToken,
        //            UserId = user.Id.ToString(),
        //            Email = user.Email,
        //        };
        //    }


        public async Task<LoginResponseDto?> RegisterUserFromMagicLinkAsync(string userId, string token)
        {
            if (!Guid.TryParse(userId, out Guid guid))
                return null;

            var confirmed = await _userService.ConfirmEmailAsync(guid, token);

            if (!confirmed.Success)
                return null;

            var user = await _userService.GetUserByIdentityAsync(userId);

            if (user == null)
                return null;

            return await GenerateTokensForUserAsync(user);
        }



        public async Task<LoginResponseDto?> RegisterWithPasswordAsync(RegisterDto model)
        {
            var registerResult = await _userService.RegisterAsync(model);
            if (!registerResult.Success) return null;

            return await LoginAsync(model.Email, model.Password);
        }

        public async Task<LoginResponseDto?> LoginAsync(string email, string password)
        {
            var loginResult = await _userService.LoginAsync(new LoginDto(email, password));
            if (!loginResult.Success) return null;

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null) return null;

            // TODO: Generate real JWT here
            return new LoginResponseDto("DEMO_JWT_TOKEN", "DEMO_REFRESH_TOKEN", user.Id.ToString());
        }

        // ===== Helper Methods =====

       public async Task<(Claim[] Claims, RegisterDto User)> GetUserWithRoles(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);

            // Retrieve roles from Identity
            var roles = await _userService.GetUserRolesAsync(user.Id.ToString());

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
    };

            // Add roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return (claims.ToArray(), user);
        }


        public async Task<LoginResponseDto> GenerateTokensForUserAsync(ApplicationUser user)
        {
            // 👇 هات الـ claims والـ roles
            var roles = await _userService.GetUserRolesAsync(user.Id.ToString());

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email)
    };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            // 👇 Generate Access Token
            var accessToken = _tokenService.GenerateAccessToken(claims);

            // 👇 Generate Refresh Token
            var refreshToken = _tokenService.GenerateRefreshToken();

            // 👇 Store refresh token in database
            await _refreshTokenService.Refresh(new RefreshTokenDto
            {
                Token = refreshToken,
                UserId = user.Id.ToString(),
                Expires = DateTime.UtcNow.AddDays(7),
                CurrentState = 1
            });

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Email = user.Email,
                UserId = user.Id.ToString(),
            };
        }


    }
}

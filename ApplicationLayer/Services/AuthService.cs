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

        //        public async Task<bool> SendMagicLinkAsync(string email)
        //        {
        //            var (result, token) = await _userService.CreateUserWithoutPasswordAndGetTokenAsync(email);

        //            if (!result.Success || token == null) return false;

        //            var frontendUrl = _config["Frontend:BaseUrl"]; // <-- خد URL من settings

        //            //var magicLink = $"{frontendUrl}?token={Uri.EscapeDataString(token)}";
        //            var magicLink = $"{frontendUrl}?userId={result.Id}&token={Uri.EscapeDataString(token)}";

        //            string emailBody = $@"
        //<!DOCTYPE html>
        //<html>
        //<head>
        //    <meta charset=""utf-8"">
        //    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        //    <title>Netflix  - Confirm your email</title>
        //    <style>
        //        body {{
        //            margin: 0;
        //            padding: 0;
        //            background-color: #f4f4f4;
        //            font-family: Helvetica, Arial, sans-serif;
        //        }}
        //        .container {{
        //            max-width: 600px;
        //            margin: 0 auto;
        //            background-color: #ffffff;
        //        }}
        //        .header {{
        //            background-color: #E50914; /* Netflix Red */
        //            padding: 30px 20px;
        //            text-align: center;
        //        }}
        //        .header img {{
        //            height: 40px;
        //        }}
        //        .content {{
        //            padding: 40px 40px 60px;
        //            text-align: center;
        //            color: #333333;
        //        }}
        //        .content h1 {{
        //            font-size: 28px;
        //            margin-bottom: 20px;
        //            color: #000000;
        //        }}
        //        .content p {{
        //            font-size: 16px;
        //            line-height: 1.5;
        //            margin-bottom: 30px;
        //            color: #555555;
        //        }}
        //        .button {{
        //            display: inline-block;
        //            background-color: #E50914;
        //            color: #ffffff !important;
        //            font-size: 18px;
        //            font-weight: bold;
        //            padding: 16px 36px;
        //            text-decoration: none;
        //            border-radius: 4px;
        //            margin: 20px 0;
        //        }}
        //        .button:hover {{
        //            background-color: #c30812;
        //        }}
        //        .footer {{
        //            background-color: #f4f4f4;
        //            padding: 30px;
        //            text-align: center;
        //            font-size: 12px;
        //            color: #999999;
        //        }}
        //        .footer a {{
        //            color: #999999;
        //            text-decoration: underline;
        //        }}
        //    </style>
        //</head>
        //<body>
        //    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f4f4;"">
        //        <tr>
        //            <td align=""center"">
        //                <table class=""container"">
        //                    <!-- Header -->
        //                    <tr>
        //                        <td class=""header"">
        //                            <!-- لو عندك لوجو WatchMe حطه هنا، لو مفيش هنكتب الاسم بالأحمر -->
        //                            <h1 style=""margin:0; color:#ffffff; font-size:32px; font-weight:bold;"">Netflix</h1>
        //                        </td>
        //                    </tr>

        //                    <!-- Content -->
        //                    <tr>
        //                        <td class=""content"">
        //                            <h1>Just one more step...</h1>
        //                            <p>You're almost ready to start enjoying Netflix.</p>
        //                            <p>Simply click the big red button below to confirm your email address and complete your registration.</p>

        //                            <a href=""{magicLink}"" class=""button"">Confirm My Email</a>

        //                            <p style=""margin-top:40px; font-size:14px; color:#777;"">
        //                                If you didn't create an account with WatchMe, you can safely ignore this email.
        //                            </p>
        //                        </td>
        //                    </tr>

        //                    <!-- Footer -->
        //                    <tr>
        //                        <td class=""footer"">
        //                            <p>© 2025 WatchMe. All rights reserved.</p>
        //                            <p>
        //                                If you're having trouble clicking the button, copy and paste this link into your browser:<br/>
        //                                <a href=""{magicLink}"">{magicLink}</a>
        //                            </p>
        //                        </td>
        //                    </tr>
        //                </table>
        //            </td>
        //        </tr>
        //    </table>
        //</body>
        //</html>";

        //            await _emailService.SendEmailAsync(email, "WatchMe Sign-Up Confirmation", emailBody);

        //            return true;
        //        }



        public async Task<bool> SendMagicLinkAsync(string email)
        {
            var (result, token) = await _userService.CreateUserWithoutPasswordAndGetTokenAsync(email);

            if (!result.Success || token == null) return false;

            var frontendUrl = _config["Frontend:BaseUrl"]; // <-- خد URL من settings

            //var magicLink = $"{frontendUrl}?token={Uri.EscapeDataString(token)}";
            var magicLink = $"{frontendUrl}";

            string emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Netflix  - Confirm your email</title>
    <style>
        body {{
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
            font-family: Helvetica, Arial, sans-serif;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
        }}
        .header {{
            background-color: #E50914; /* Netflix Red */
            padding: 30px 20px;
            text-align: center;
        }}
        .header img {{
            height: 40px;
        }}
        .content {{
            padding: 40px 40px 60px;
            text-align: center;
            color: #333333;
        }}
        .content h1 {{
            font-size: 28px;
            margin-bottom: 20px;
            color: #000000;
        }}
        .content p {{
            font-size: 16px;
            line-height: 1.5;
            margin-bottom: 30px;
            color: #555555;
        }}
        .button {{
            display: inline-block;
            background-color: #E50914;
            color: #ffffff !important;
            font-size: 18px;
            font-weight: bold;
            padding: 16px 36px;
            text-decoration: none;
            border-radius: 4px;
            margin: 20px 0;
        }}
        .button:hover {{
            background-color: #c30812;
        }}
        .footer {{
            background-color: #f4f4f4;
            padding: 30px;
            text-align: center;
            font-size: 12px;
            color: #999999;
        }}
        .footer a {{
            color: #999999;
            text-decoration: underline;
        }}
    </style>
</head>
<body>
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f4f4;"">
        <tr>
            <td align=""center"">
                <table class=""container"">
                    <!-- Header -->
                    <tr>
                        <td class=""header"">
                            <!-- لو عندك لوجو WatchMe حطه هنا، لو مفيش هنكتب الاسم بالأحمر -->
                            <h1 style=""margin:0; color:#ffffff; font-size:32px; font-weight:bold;"">Netflix</h1>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td class=""content"">
                            <h1>Just one more step...</h1>
                            <p>You're almost ready to start enjoying Netflix.</p>
                            <p>Simply click the big red button below to confirm your email address and complete your registration.</p>
                            
                            <a href=""{magicLink}"" class=""button"">Confirm My Email</a>
                            
                            <p style=""margin-top:40px; font-size:14px; color:#777;"">
                                If you didn't create an account with WatchMe, you can safely ignore this email.
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td class=""footer"">
                            <p>© 2025 WatchMe. All rights reserved.</p>
                            <p>
                                If you're having trouble clicking the button, copy and paste this link into your browser:<br/>
                                <a href=""{magicLink}"">{magicLink}</a>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            await _emailService.SendEmailAsync(email, "WatchMe Sign-Up Confirmation", emailBody);

            return true;
        }



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

        public async Task<LoginResponseDto?> ConfirmEmailAndGenerateTokensAsync(string userId, string token)
        {
            // تأكيد الايميل
            var confirm = await _userService.ConfirmEmailAsync(Guid.Parse(userId), token);
            if (!confirm.Success) return null;

            // جلب الـ user
            var user = await _userService.GetUserByIdentityAsync(userId);
            if (user == null) return null;

            // جلب الأدوار
            var roles = await _userService.GetUserRolesAsync(userId);

            // إعداد الـ claims
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            // توليد الـ Access Token
            var accessToken = _tokenService.GenerateAccessToken(claims);

            // توليد الـ Refresh Token
            var refreshToken = _tokenService.GenerateRefreshToken();

            // حفظ الـ Refresh Token في DB
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
                UserId = user.Id.ToString(),
                Email = user.Email
            };
        }


        public async Task<LoginResponseDto?> RegisterAndGenerateTokensAsync(RegisterDto dto)
        {
            var result = await _userService.RegisterAsync(dto);

            if (!result.Success)
                return null;

            var user = await _userService.GetUserByEmailAsync(dto.Email);
            if (user == null) return null;

            var (claims, appUser) = await GetUserWithRoles(dto.Email);

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            await _refreshTokenService.Refresh(new RefreshTokenDto
            {
                Token = refreshToken,
                UserId = appUser.Id.ToString(),
                Expires = DateTime.UtcNow.AddDays(7),
                CurrentState = 1
            });

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = appUser.Id.ToString(),
                Email = appUser.Email
            };
        }



    }
}

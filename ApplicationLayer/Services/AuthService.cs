using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using InfrastructureLayer.UserModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Presentation.Services;
using System.Security.Claims;

namespace ApplicationLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly EmailService _emailService;
        private readonly TokenService _tokenService;
        private readonly IRefreshTokens _refreshTokenService;
        private readonly IOtpRepository _otpRepository;
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(
            IUserService userService,
            EmailService emailService,
            TokenService tokenService,
            IRefreshTokens refreshTokenService,
            IOtpRepository otpRepository,
            IConfiguration config,
            UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _emailService = emailService;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _otpRepository = otpRepository;
            _config = config;
            _userManager = userManager;
        }

        // ==========================
        // Send Magic Link
        // ==========================
//        public async Task<bool> SendMagicLinkAsync(string email)
//        {
//            var (result, token) = await _userService.CreateUserWithoutPasswordAndGetTokenAsync(email);
//            if (!result.Success || token == null) return false;

//            var frontendUrl = _config["Frontend:BaseUrl"];
//            var magicLink = $"{frontendUrl}?userId={result.Id}&token={Uri.EscapeDataString(token)}";

//            string emailBody = $@"
//<!DOCTYPE html>
//<html>
//<head>
//    <meta charset=""utf-8"">
//    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
//    <title>WatchMe - Confirm your email</title>
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
//                            <h1 style=""margin:0; color:#ffffff; font-size:32px; font-weight:bold;"">WatchMe</h1>
//                        </td>
//                    </tr>

//                    <!-- Content -->
//                    <tr>
//                        <td class=""content"">
//                            <h1>Just one more step...</h1>
//                            <p>You're almost ready to start enjoying WatchMe.</p>
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

//            await _emailService.SendEmailAsync(email, "Confirm your email", emailBody);
//            return true;
//        }

//        public async Task<bool> SendMagicLinkAsync(string email)
//        {
//            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

//            await _otpRepository.SaveOtpAsync(email, token, DateTime.UtcNow.AddMinutes(10));

//            var frontendUrl = _config["Frontend:BaseUrl"];
//            var magicLink = $"{frontendUrl}?email={email}&token={Uri.EscapeDataString(token)}";
//            string emailBody = $@"
//<!DOCTYPE html>
//<html>
//<head>
//    <meta charset=""utf-8"">
//    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
//    <title>WatchMe - Confirm your email</title>
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
//                            <h1 style=""margin:0; color:#ffffff; font-size:32px; font-weight:bold;"">WatchMe</h1>
//                        </td>
//                    </tr>

//                    <!-- Content -->
//                    <tr>
//                        <td class=""content"">
//                            <h1>Just one more step...</h1>
//                            <p>You're almost ready to start enjoying WatchMe.</p>
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

//            await _emailService.SendEmailAsync(email, "Confirm your email", emailBody);

//            return true;
//        }


        // إرسال رابط التفعيل مع البريد الإلكتروني
        public async Task<bool> SendMagicLinkAsync(string email)
        {
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            // حفظ التوكن في قاعدة البيانات
            await _otpRepository.SaveOtpAsync(email, token, DateTime.UtcNow.AddMinutes(10));

            var frontendUrl = _config["Frontend:BaseUrl"];
            var magicLink = $"{frontendUrl}?email={email}&token={Uri.EscapeDataString(token)}";

            string emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>WatchMe - Confirm your email</title>
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
                            <h1 style=""margin:0; color:#ffffff; font-size:32px; font-weight:bold;"">WatchMe</h1>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td class=""content"">
                            <h1>Just one more step...</h1>
                            <p>You're almost ready to start enjoying WatchMe.</p>
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

            // إرسال البريد الإلكتروني
            await _emailService.SendEmailAsync(email, "Confirm your email", emailBody);

            return true;
        }


        // ==========================
        // Register with Password + Tokens
        // ==========================
        public async Task<LoginResponseDto?> RegisterAndGenerateTokensAsync(RegisterDto dto)
        {
            var result = await _userService.RegisterAsync(dto);
            if (!result.Success) return null;

            var user = await _userService.GetUserByEmailAsyncs(dto.Email);
            if (user == null) return null;

            return await GenerateTokensForUserAsync(user);
        }

        // ==========================
        // Login with Password + Tokens
        // ==========================
        public async Task<LoginResponseDto?> LoginAndGenerateTokensAsync(string email, string password)
        {
            var loginResult = await _userService.LoginAsync(new LoginDto(email, password));
            if (!loginResult.Success) return null;

            var user = await _userService.GetUserByEmailAsyncs(email);
            if (user == null) return null;

            return await GenerateTokensForUserAsync(user);
        }

        //==========================
        //Confirm Email + Tokens
        //==========================
        //public async Task<LoginResponseDto?> ConfirmEmailAndGenerateTokensAsync(string userId, string token)
        //{
        //    if (!Guid.TryParse(userId, out var guid)) return null;

        //    var confirm = await _userService.ConfirmEmailAsync(guid, token);
        //    if (!confirm.Success) return null;

        //    var user = await _userService.GetUserByIdentityAsync(userId);
        //    if (user == null) return null;

        //    return await GenerateTokensForUserAsync(user);
        //}



        // ==========================
        // OTP: Generate + Send
        // ==========================


        //public async Task<LoginResponseDto?> ConfirmEmailAndGenerateTokensAsync(string email, string token)
        //{
        //    // 1️⃣ Validate token from OTP table
        //    var storedToken = await _otpRepository.GetValidOtpAsync(email, token);
        //    if (storedToken == null)
        //        return null;

        //    // 2️⃣ Check if user exists or create new user
        //    var user = await _userManager.FindByEmailAsync(email);

        //    if (user == null)
        //    {
        //        user = new ApplicationUser
        //        {
        //            Email = email,
        //            UserName = email,
        //            Name = email.Split('@')[0],
        //            EmailConfirmed = false
        //        };

        //        var createResult = await _userManager.CreateAsync(user);

        //        if (!createResult.Succeeded)
        //            return null;

        //        await _userManager.AddToRoleAsync(user, "User");
        //    }

        //    // 3️⃣ Confirm email if not confirmed yet
        //    if (!user.EmailConfirmed)
        //    {
        //        var identityToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        //        var confirm = await _userManager.ConfirmEmailAsync(user, identityToken);

        //        if (!confirm.Succeeded)
        //            return null;
        //    }

        //    // 4️⃣ Mark Token As Used
        //    await _otpRepository.MarkOtpUsedAsync(email, token);

        //    // 5️⃣ Generate Auth Tokens and return response
        //    var response = await GenerateTokensForUserAsync(user);

        //    return response;
        //}


        public async Task<LoginResponseDto?> ConfirmEmailAndGenerateTokensAsync(string email, string token)
        {
            // 1️⃣ Validate token from OTP table
            var storedToken = await _otpRepository.GetValidOtpAsync(email, token);
            if (storedToken == null)
                return null;

            // 2️⃣ Check if user exists or create new user
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // Create new user if not exists
                user = new ApplicationUser
                {
                    Email = email,
                    UserName = email,
                    Name = email.Split('@')[0],
                    EmailConfirmed = false
                };

                var createResult = await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                    return null;

                await _userManager.AddToRoleAsync(user, "User");
            }

            // 3️⃣ Confirm email if not confirmed yet
            if (!user.EmailConfirmed)
            {
                var identityToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirm = await _userManager.ConfirmEmailAsync(user, identityToken);

                if (!confirm.Succeeded)
                    return null;
            }

            // 4️⃣ Mark Token As Used
            await _otpRepository.MarkOtpUsedAsync(email, token);

            // 5️⃣ Generate Auth Tokens and return response
            var response = await GenerateTokensForUserAsync(user);

            return response;
        }

        public async Task<bool> GenerateAndSendOtpAsync(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null) return false;

            // Generate 4-digit OTP
            var code = new Random().Next(1000, 10000).ToString(); // 1000 to 9999

            await _otpRepository.SaveOtpAsync(user.Id.ToString(), code, DateTime.UtcNow.AddMinutes(5));

            string emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Netflix - Your verification code</title>
    <style>
        body {{ margin:0; padding:0; background:#f4f4f4; font-family:Helvetica,Arial,sans-serif; }}
        .container {{ max-width:600px; margin:0 auto; background:#ffffff; }}
        .header {{ background:#E50914; padding:30px; text-align:center; }}
        .content {{ padding:40px 30px 60px; text-align:center; color:#333; }}
        h1 {{ font-size:28px; margin:0 0 20px; color:#000; }}
        p {{ font-size:16px; line-height:1.6; color:#555; margin:0 0 30px; }}
        .code-box {{
            display:inline-block;
            background:#000000;
            color:#ffffff;
            font-size:36px;
            font-weight:bold;
            letter-spacing:10px;
            padding:24px 48px;
            border-radius:8px;
            margin:30px 0;
            border:5px solid #E50914;
        }}
        .footer {{
            background:#f4f4f4;
            padding:30px;
            text-align:center;
            font-size:12px;
            color:#999;
        }}
        .footer a {{ color:#999; text-decoration:underline; }}
        @media (max-width: 480px) {{
            .code-box {{ font-size:28px; letter-spacing:6px; padding:20px 30px; }}
        }}
    </style>
</head>
<body>
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f4f4f4;"">
        <tr>
            <td align=""center"">
                <table class=""container"">
                    <!-- Header -->
                    <tr>
                        <td class=""header"">
                            <h1 style=""margin:0; color:#fff; font-size:38px; font-weight:bold; letter-spacing:2px;"">Netflix</h1>
                        </td>
                    </tr>

                    <!-- Content -->
                    <tr>
                        <td class=""content"">
                            <h1>Your verification code</h1>
                            <p>Use the code below to complete your sign in.</p>
                            <p>This code expires in 10 minutes.</p>

                            <!-- الكود البارز فقط -->
                            <div class=""code-box"">{code}</div>

                            <p style=""font-size:14px; color:#777; margin-top:50px;"">
                                If you didn’t request this code, you can safely ignore this email. 
                                Someone else might have entered your email address by mistake.
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td class=""footer"">
                            <p>© 2025 Netflix, Inc. All rights reserved.</p>
                            <p>This is an automated message, please do not reply.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            await _emailService.SendEmailAsync(email, "Your OTP Code", emailBody);

            return true;
        }


        // ==========================
        // Login with OTP + Tokens
        // ==========================
        public async Task<LoginResponseDto?> LoginWithOtpAndGenerateTokensAsync(string email, string code)
        {
            var user = await _userService.GetUserByEmailAsyncs(email);
            if (user == null) return null;

            var valid = await _otpRepository.ValidateOtpAsync(user.Id.ToString(), code);
            if (!valid) return null;

            return await GenerateTokensForUserAsync(user);
        }

        // ==========================
        // Refresh Access Token
        // ==========================
        public async Task<LoginResponseDto?> RefreshAccessTokenAsync(string refreshToken)
        {
            var stored = await _refreshTokenService.GetByTokenAsync(refreshToken);
            if (stored == null || stored.Expires < DateTime.UtcNow) return null;

            var user = await _userService.GetUserByIdentityAsync(stored.UserId);
            if (user == null) return null;

            return await GenerateTokensForUserAsync(user);
        }

        // ==========================
        // Helper: Generate Tokens for User
        // ==========================
        private async Task<LoginResponseDto> GenerateTokensForUserAsync(ApplicationUser user)
        {
            // Get roles
            var roles = await _userService.GetUserRolesAsync(user.Id.ToString());

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email)
    };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token
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

        public Task<(Claim[] Claims, RegisterDto User)> GetUserWithRolesAsync(string email)
        {
            throw new NotImplementedException();
        }





        //public Task<LoginResponseDto?> RegisterUserFromMagicLinkAsync(string userId, string token)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<LoginResponseDto?> RegisterWithPasswordAsync(RegisterDto model)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<LoginResponseDto?> LoginAsync(string email, string password)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<(Claim[] Claims, RegisterDto User)> GetUserWithRoles(string email)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<LoginResponseDto> GenerateTokensForUserAsync(ApplicationUser user)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<string> IAuthService.GenerateAndSendOtpAsync(string email)
        //{
        //    throw new NotImplementedException();
        //}
    }
}

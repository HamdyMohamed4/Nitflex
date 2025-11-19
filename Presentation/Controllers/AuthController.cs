using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using ApplicationLayer.Services;
using InfrastructureLayer.UserModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using Presentation.Services;
using System.Security.Claims;


namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly EmailService _emailService;
        private readonly TokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IOtpRepository _otpRepository;
        private readonly IRefreshTokens _RefreshTokenService;
        private readonly IRefreshTokenRetriver _RefreshTokenRetriver;

        public AuthController(IAuthService authService, TokenService tokenService,
                              IUserService userService,
                              IRefreshTokens refreshTokenService,
                              IRefreshTokenRetriver refreshTokenRetriver,
                              EmailService emailService,
                              IOtpRepository otpRepository)
        {
            _authService = authService;
            _tokenService = tokenService;
            _userService = userService;
            _RefreshTokenService = refreshTokenService;
            _RefreshTokenRetriver = refreshTokenRetriver;
            _emailService = emailService;
            _otpRepository = otpRepository;

        }

        [HttpPost("check-email")]
        public async Task<ActionResult<ApiResponse<object>>> CheckEmail([FromBody] EmailRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.FailResponse("Validation failed.", errors));
            }

            var user = await _userService.GetUserByEmailAsync(model.Email);
            if (user != null)
            {
                // إذا كان المستخدم موجودًا، توجهه إلى صفحة تسجيل الدخول
                return Ok(ApiResponse<object>.SuccessResponse(null, "User already exists, redirecting to login."));
            }
            else
            {
                // إذا كان المستخدم غير موجود، أكمل التسجيل
                return Ok(ApiResponse<object>.SuccessResponse(null, "Proceeding with registration."));
            }
        }

        [HttpPost("send-magic-link")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> SendMagicLink([FromBody] EmailRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Validation failed.", errors));
            }

            var success = await _authService.SendMagicLinkAsync(model.Email);

            if (!success)
                return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Could not send link."));

            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(null, $"Sign-up link sent to {model.Email}"));
        }


        [HttpGet("confirm-signup")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> ConfirmSignUp(string userId, string token)
        {
            var confirm = await _userService.ConfirmEmailAsync(Guid.Parse(userId), token);

            if (!confirm.Success)
                return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid or expired token."));

            var user = await _userService.GetUserByIdAsync(userId);

            return await GenerateAuthTokensAsync(user!.Email);
        }



        //[HttpGet("confirm-signup")]
        //public async Task<ActionResult<ApiResponse<LoginResponseDto>>> ConfirmSignUp([FromQuery] string userId, [FromQuery] string token)
        //{
        //    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        //        return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid confirmation link."));

        //    var response = await _authService.RegisterUserFromMagicLinkAsync(userId, token);
        //    if (response != null)
        //        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "Registration confirmed. Proceed to plan selection."));

        //    return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid or expired token."));
        //}

        //[HttpPost("register-with-password")]
        //public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RegisterWithPassword([FromBody] RegisterDto model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        //        return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Validation failed.", errors));
        //    }

        //    var response = await _authService.RegisterWithPasswordAsync(model);
        //    if (response != null)
        //        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "Account created."));

        //    return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Registration failed."));
        //}

        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] LoginDto request)
        //{
        //    var userResult = await _userService.LoginAsync(request);
        //    if (!userResult.Success)
        //    {
        //        return Unauthorized("Invalid credentials");
        //    }


        //    var userData = await GetClims(request.Email);
        //    var claims = userData.Item1;
        //    RegisterDto user = userData.Item2;
        //    var accessToken = _tokenService.(claims);
        //    var refreshToken = _tokenService.GenerateRefreshToken();

        //    var storedToken = new RefreshTokenDto
        //    {
        //        Token = refreshToken,
        //        UserId = user.Id.ToString(),
        //        Expires = DateTime.UtcNow.AddDays(7),
        //        CurrentState = 1
        //    };

        //    await _RefreshTokenService.Refresh(storedToken);

        //    Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
        //    {
        //        HttpOnly = true,
        //        Secure = true,
        //        Expires = storedToken.Expires
        //    });

        //    return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
        //}

        //[HttpPost("confirm-signup")]
        //public async Task<ActionResult<ApiResponse<LoginResponseDto>>> ConfirmSignUp([FromBody] ConfirmMagicLinkDto dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid input"));

        //    // كل المنطق بقى جوه AuthService
        //    var authResponse = await _authService.ConfirmEmailAndGenerateTokensAsync(dto.UserId, dto.Token);

        //    if (authResponse == null)
        //        return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid or expired token."));

        //    // Optional: وضع الـ Refresh Token في Cookie
        //    Response.Cookies.Append("RefreshToken", authResponse.RefreshToken, new CookieOptions
        //    {
        //        HttpOnly = true,
        //        Secure = true,
        //        Expires = DateTime.UtcNow.AddDays(7)
        //    });

        //    return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(authResponse, "Email confirmed and user logged in."));
        //}






        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterDto dto)
        {
            var result = await _userService.RegisterAsync(dto);

            if (!result.Success)
                return BadRequest(result.Errors);

            return await GenerateAuthTokensAsync(dto.Email);
        }






        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var userResult = await _userService.LoginAsync(request);
            if (!userResult.Success)
            {
                return Unauthorized("Invalid credentials");
            }

            // 👇 الجديد: احصل على الـ Claims من خلال AuthService
            var (claims, user) = await _authService.GetUserWithRoles(request.Email);

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var storedToken = new RefreshTokenDto
            {
                Token = refreshToken,
                UserId = user.Id.ToString(),
                Expires = DateTime.UtcNow.AddDays(7),
                CurrentState = 1
            };

            await _RefreshTokenService.Refresh(storedToken);

            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = storedToken.Expires
            });

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
            });
        }



        [HttpPost("request-otp")]
        public async Task<ActionResult<ApiResponse<object>>> RequestOtp([FromBody] EmailRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.FailResponse("Validation failed.", errors));
            }

            // توليد كود OTP
            var otp = new Random().Next(1000, 9999).ToString();

            // حفظ الكود في DB
            await _otpRepository.SaveOtpAsync(new OtpDto
            {
                Email = model.Email,
                OtpCode = otp,
                ExpirationDate = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            });

            // ارسال الكود عبر البريد
            await _emailService.SendEmailAsync(
                model.Email,
                "Your Login Verification Code",
                $"Your OTP Code is: <b>{otp}</b><br/><br/>This code expires in 5 minutes."
            );

            return Ok(ApiResponse<object>.SuccessResponse(null, $"OTP sent to {model.Email}."));
        }

        //// === Login using OTP ===
        //[HttpPost("login-otp")]
        //public async Task<ActionResult<ApiResponse<LoginResponseDto>>> LoginOtp([FromBody] LoginWithOtpDto dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid Request"));

        //    var result = await _userService.LoginWithOtpAsync(dto.Email, dto.Code);

        //    if (result == null)
        //        return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid or expired OTP."));

        //    return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(result, "Login Successful"));
        //}

        //    [HttpPost("login-otp")]
        //    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> LoginOtp([FromBody] LoginWithOtpDto dto)
        //    {
        //        if (!ModelState.IsValid)
        //            return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid Request"));

        //        var user = await _userService.LoginWithOtpAsync(dto.Email, dto.Code);

        //        if (user == null || string.IsNullOrEmpty(user.UserId) || string.IsNullOrEmpty(user.Email))
        //            return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid or expired OTP. User missing required data."));

        //        var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.NameIdentifier, user.UserId),
        //    new Claim(ClaimTypes.Email, user.Email),
        //    new Claim(ClaimTypes.Role, "User")
        //};

        //        var accessToken = _tokenService.GenerateAccessToken(claims);
        //        var refreshToken = _tokenService.GenerateRefreshToken();

        //        await _RefreshTokenService.Refresh(new RefreshTokenDto
        //        {
        //            Token = refreshToken,
        //            UserId = user.UserId,
        //            Expires = DateTime.UtcNow.AddDays(7),
        //            CurrentState = 1
        //        });

        //        Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
        //        {
        //            HttpOnly = true,
        //            Secure = true,
        //            SameSite = SameSiteMode.Strict,
        //            Expires = DateTime.UtcNow.AddDays(7)
        //        });

        //        user.AccessToken = accessToken;
        //        user.RefreshToken = refreshToken;

        //        return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(user, "Login successful with OTP."));
        //    }

        [HttpPost("login-otp")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> LoginOtp([FromBody] LoginWithOtpDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid Request"));

            var user = await _userService.LoginWithOtpAsync(dto.Email, dto.Code);

            if (user == null || string.IsNullOrEmpty(user.UserId))
                return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid or expired OTP."));

            // 👇 الجديد: اجلب Claims داخل الـ AuthService
            var (claims, userObj) = await _authService.GetUserWithRoles(user.Email);

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            await _RefreshTokenService.Refresh(new RefreshTokenDto
            {
                Token = refreshToken,
                UserId = user.UserId,
                Expires = DateTime.UtcNow.AddDays(7),
                CurrentState = 1
            });

            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            user.AccessToken = accessToken;
            user.RefreshToken = refreshToken;

            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(user, "Login successful with OTP."));
        }


        [HttpPost("RefreshAccessToken")]
        public async Task<IActionResult> RefreshAccessToken()
        {
            if (!Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
            {
                return Unauthorized("No refresh token found");
            }

            // Retrieve the refresh token from the database
            var storedToken = await _RefreshTokenRetriver.GetByToken(refreshToken);
            if (storedToken == null || storedToken.CurrentState == 2 || storedToken.Expires < DateTime.UtcNow)
            {
                return Unauthorized("Invalid or expired refresh token");
            }

            // Generate a new access token
            var claims = await GetClimsById(storedToken.UserId);

            var newAccessToken = _tokenService.GenerateAccessToken(claims);

            Response.Cookies.Append("AccessToken", newAccessToken, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                Expires = DateTime.UtcNow.AddMinutes(15)  // Adjust token expiry based on your needs
            });

            return Ok(new { AccessToken = newAccessToken });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
            {
                return Unauthorized("No refresh token found");
            }

            // Retrieve the refresh token from the database
            var storedToken = await _RefreshTokenRetriver.GetByToken(refreshToken);
            if (storedToken == null || storedToken.CurrentState == 2 || storedToken.Expires < DateTime.UtcNow)
            {
                return Unauthorized("Invalid or expired refresh token");
            }

            // Generate a new refresh token
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var newRefreshDto = new RefreshTokenDto
            {
                Token = newRefreshToken,
                UserId = storedToken.Id.ToString(),
                Expires = DateTime.UtcNow.AddDays(7),
                CurrentState = 1
            };
            await _RefreshTokenService.Refresh(newRefreshDto);

            // Set the new refresh token in the cookies
            Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new { RefreshToken = newRefreshToken });
        }

        //async Task<(Claim[], RegisterDto)> GetClims(string email)
        //{
        //    var user = await _userService.GetUserByEmailAsync(email);
        //    var claims = new[] {
        //        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        //        new Claim(ClaimTypes.Name, user.Email),
        //        new Claim(ClaimTypes.Role, "User")
        //    };

        //    return (claims, user);
        //}

        async Task<Claim[]> GetClimsById(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, "User")
            };

            return claims;
        }


        private async Task<ActionResult<ApiResponse<LoginResponseDto>>> GenerateAuthTokensAsync(string email)
        {
            var (claims, user) = await _authService.GetUserWithRoles(email);

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var storedToken = new RefreshTokenDto
            {
                Token = refreshToken,
                UserId = user.Id.ToString(),
                Expires = DateTime.UtcNow.AddDays(7),
                CurrentState = 1
            };

            await _RefreshTokenService.Refresh(storedToken);

            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = storedToken.Expires
            });

            var response = new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = user.Id.ToString(),
                Email = user.Email
            };

            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "User authenticated successfully."));
        }




    }
}

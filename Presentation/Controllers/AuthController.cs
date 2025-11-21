using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using ApplicationLayer.Services;
using InfrastructureLayer.UserModels;
using Microsoft.AspNetCore.Http;
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
        private readonly IUserService _userService;
        private readonly IRefreshTokens _refreshTokenService;
        private readonly IRefreshTokenRetriver _refreshTokenRetriver;
        private readonly EmailService _emailService;
        private readonly TokenService _tokenService;
        private readonly IOtpRepository _otpRepository;

        public AuthController(
            IAuthService authService,
            IUserService userService,
            IRefreshTokens refreshTokenService,
            IRefreshTokenRetriver refreshTokenRetriver,
            EmailService emailService,
            TokenService tokenService,
            IOtpRepository otpRepository)
        {
            _authService = authService;
            _userService = userService;
            _refreshTokenService = refreshTokenService;
            _refreshTokenRetriver = refreshTokenRetriver;
            _emailService = emailService;
            _tokenService = tokenService;
            _otpRepository = otpRepository;
        }

        // ============================
        // Check if email exists
        // ============================
        [HttpPost("check-email")]
        public async Task<ActionResult<ApiResponse<object>>> CheckEmail([FromBody] EmailRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.FailResponse("Validation failed.", errors));
            }


            var user = await _userService.GetUserByEmailAsync(model.Email);
            var message = user != null ? "User already exists, redirecting to login." : "Proceeding with registration.";
            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        // ============================
        // Send Magic Link Email
        // ============================
        [HttpPost("send-magic-link")]
        public async Task<ActionResult<ApiResponse<object>>> SendMagicLink([FromBody] EmailRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.FailResponse("Validation failed.", errors));
            }


            var success = await _authService.SendMagicLinkAsync(model.Email);
            if (!success)
                return BadRequest(ApiResponse<object>.FailResponse("Could not send magic link."));

            return Ok(ApiResponse<object>.SuccessResponse(null, $"Sign-up link sent to {model.Email}"));
            
        }



        // ============================
        // Confirm Email & Generate Tokens
        // ============================      

        //[HttpPost("confirm-signup")]
        //public async Task<ActionResult<ApiResponse<LoginResponseDto>>> ConfirmSignUp([FromBody] ConfirmMagicLinkDto dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid input"));

        //    var user = await _authService.ConfirmEmailAndGenerateTokensAsync(dto.Email, dto.Token);
        //    if (user == null)
        //        return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid or expired token."));

        //    SetRefreshTokenCookie(user.RefreshToken);
        //    return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(user, "Email confirmed and user logged in."));
        //}

        [HttpPost("confirm-signup")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> ConfirmSignUp([FromBody] ConfirmMagicLinkDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid input", null));

            // Decode token because it arrives URL encoded
            dto.Token = Uri.UnescapeDataString(dto.Token);

            // Step 1: Confirm & Create/Login User
            var userResponse = await _authService.ConfirmEmailAndGenerateTokensAsync(dto.Email, dto.Token);

            if (userResponse == null)
                return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid or expired token.", null));

            // Step 2: Store refresh token in HttpOnly cookie
            SetRefreshTokenCookie(userResponse.RefreshToken);

            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(
                userResponse,
                "Email confirmed successfully and user logged in."
            ));
        }


        // ============================
        // Register & Generate Tokens
        // ============================

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid input"));

            var user = await _authService.RegisterAndGenerateTokensAsync(dto);
            if (user == null)
                return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Registration failed."));

            SetRefreshTokenCookie(user.RefreshToken);
            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(user, "User registered successfully."));
        }

        // ============================
        // Login
        // ============================
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid input"));

            var user = await _authService.LoginAndGenerateTokensAsync(dto.Email, dto.Password);
            if (user == null)
                return Unauthorized(ApiResponse<LoginResponseDto>.FailResponse("Invalid credentials"));

            SetRefreshTokenCookie(user.RefreshToken);
            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(user, "Login successful."));
        }

        // ============================
        // Request OTP
        // ============================
        [HttpPost("request-otp")]
        public async Task<ActionResult<ApiResponse<object>>> RequestOtp([FromBody] EmailRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.FailResponse("Validation failed.", errors));
            }


            var otp = await _authService.GenerateAndSendOtpAsync(model.Email);
            return Ok(ApiResponse<object>.SuccessResponse(null, $"OTP sent to {model.Email}."));
        }

        // ============================
        // Login with OTP
        // ============================
        [HttpPost("login-otp")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> LoginOtp([FromBody] LoginWithOtpDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid input"));

            var user = await _authService.LoginWithOtpAndGenerateTokensAsync(dto.Email, dto.Code);
            if (user == null)
                return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid or expired OTP."));

            SetRefreshTokenCookie(user.RefreshToken);
            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(user, "Login successful with OTP."));
        }

        // ============================
        // Refresh Access Token
        // ============================
        [HttpPost("refresh-access-token")]
        public async Task<IActionResult> RefreshAccessToken()
        {
            if (!Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
                return Unauthorized("No refresh token found.");

            var user = await _authService.RefreshAccessTokenAsync(refreshToken);
            if (user == null)
                return Unauthorized("Invalid or expired refresh token.");

            return Ok(new { AccessToken = user.AccessToken });
        }

        // ============================
        // Private Helper: Set Refresh Token Cookie
        // ============================
        private void SetRefreshTokenCookie(string refreshToken)
        {
            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.UtcNow.AddDays(7)
            });
        }
    }
}

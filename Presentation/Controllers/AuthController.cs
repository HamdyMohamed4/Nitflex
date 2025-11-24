using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using ApplicationLayer.Services;
using InfrastructureLayer.UserModels;
using Microsoft.AspNetCore.Authorization;
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

            var usert = await _userService.GetUserByEmailAsync(model.Email);

            // إذا كان المستخدم محظورًا
            if (usert == null)
            {
                // إرجاع رسالة تفيد بأن المستخدم محظور
                return BadRequest(ApiResponse<object>.FailResponse("User is blocked.", new List<string> { "This user is blocked and cannot proceed." }));
            }

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
            var usert = await _userService.GetUserByEmailAsync(dto.Email);

            // إذا كان المستخدم محظورًا
            if (usert == null)
            {
                // إرجاع رسالة تفيد بأن المستخدم محظور
                return BadRequest(ApiResponse<object>.FailResponse("User is blocked.", new List<string> { "This user is blocked and cannot proceed." }));
            }
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid input"));

            var user = await _authService.LoginAndGenerateTokensAsync(dto.Email, dto.Password);
            if (user == null)
                return Unauthorized(ApiResponse<LoginResponseDto>.FailResponse("Invalid credentials"));

            SetRefreshTokenCookie(user.RefreshToken);
            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(user, "Login successful."));
        }



        // Logout (JWT) - invalidate refresh token server-side and remove cookie
        // ============================
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<object>>> Logout()
        {
            try
            {
                // If client stored refresh token in HTTP-only cookie, remove it and invalidate server-side record.
                if (Request.Cookies.TryGetValue("RefreshToken", out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
                {
                    var tokenRecord = await _refreshTokenService.GetByTokenAsync(refreshToken);
                    if (tokenRecord != null)
                    {
                        // mark refresh token as inactive (soft delete)
                        await _refreshTokenService.ChangeStatus(tokenRecord.Id, 0);
                    }

                    // Remove cookie from client
                    Response.Cookies.Delete("RefreshToken");
                }

                // Also sign out from server auth mechanisms (no-op for pure JWT but safe if cookie auth used)
                await _userService.LogoutAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null, "Logged out successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.FailResponse("Logout failed.", new List<string> { ex.Message }));
            }
        }

        // ============================
        // Request OTP
        // ============================
        [HttpPost("request-otp")]
        public async Task<ActionResult<ApiResponse<object>>> RequestOtp([FromBody] EmailRequestDto model)
        {

            var usert = await _userService.GetUserByEmailAsync(model.Email);

            // إذا كان المستخدم محظورًا
            if (usert == null)
            {
                // إرجاع رسالة تفيد بأن المستخدم محظور
                return BadRequest(ApiResponse<object>.FailResponse("User is blocked.", new List<string> { "This user is blocked and cannot proceed." }));
            }
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



        // i want you to make a new endpoint called change-password that takes in the old password and the new password and changes the password for the logged in user. make sure to validate the old password before changing to the new password. return appropriate responses for success and failure cases.
        [HttpPost("change-password")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailResponse("Invalid input"));
            // Get the logged-in user's ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<object>.FailResponse("User not logged in"));
            var result = await _authService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);
            if (!result)
                return BadRequest(ApiResponse<object>.FailResponse("Old password is incorrect or password change failed"));
            return Ok(ApiResponse<object>.SuccessResponse(null, "Password changed successfully"));
        }






        [Authorize(Roles = "User")]
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] EmailRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailResponse("Validation failed."));

            var user = await _userService.GetUserByEmailAsync(model.Email);
            if (user == null)
                return BadRequest(ApiResponse<object>.FailResponse("User not found."));

            var resetToken = await _authService.GeneratePasswordResetTokenAsync(model.Email);
            if (string.IsNullOrEmpty(resetToken))
                return BadRequest(ApiResponse<object>.FailResponse("Could not generate password reset token."));

            await _authService.SendPasswordResetEmailAsync(model.Email, resetToken);

            return Ok(ApiResponse<object>.SuccessResponse(null, $"Password reset token sent to {model.Email}."));
        }



        [Authorize(Roles = "User")]
        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailResponse("Invalid input"));

            // Decode URL encoded token
            dto.Token = Uri.UnescapeDataString(dto.Token);

            var success = await _authService.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword);
            if (!success)
                return BadRequest(ApiResponse<object>.FailResponse("Invalid OTP or expired token."));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Password reset successfully."));
        }



        //[HttpPost("forgot-password")]
        //[Authorize(Roles = "User")]
        //public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] EmailRequestDto model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        //        return BadRequest(ApiResponse<object>.FailResponse("Validation failed.", errors));
        //    }
        //    var user = await _userService.GetUserByEmailAsync(model.Email);
        //    if (user == null)
        //        return BadRequest(ApiResponse<object>.FailResponse("User not found."));
        //    var resetToken = await _authService.GeneratePasswordResetTokenAsync(user.Email);
        //    if (string.IsNullOrEmpty(resetToken))
        //        return BadRequest(ApiResponse<object>.FailResponse("Could not generate password reset token."));
        //    // Send the reset token via email
        //    await _authService.SendPasswordResetEmailAsync(model.Email, resetToken);
        //    return Ok(ApiResponse<object>.SuccessResponse(null, $"Password reset token sent to {model.Email}."));

        //}


        //[HttpPost("reset-password")]
        //public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordDto dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ApiResponse<object>.FailResponse("Invalid input"));

        //    // Decode token because it arrives URL encoded
        //    dto.Token = Uri.UnescapeDataString(dto.Token);

        //    var resetStatus = await _userService.ResetPasswordAsync(
        //        dto.Email,
        //        dto.Token,
        //        dto.NewPassword
        //    );

        //    if (!resetStatus)
        //        return BadRequest(ApiResponse<object>.FailResponse("Invalid OTP or expired token."));

        //    return Ok(ApiResponse<object>.SuccessResponse(null, "Password reset successfully."));
        //}




    }
}


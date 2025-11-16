using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using ApplicationLayer.Services;
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
        private readonly TokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IRefreshTokens _RefreshTokenService;
        private readonly IRefreshTokenRetriver _RefreshTokenRetriver;

        public AuthController(IAuthService authService, TokenService tokenService,
                              IUserService userService,
                              IRefreshTokens refreshTokenService,
                              IRefreshTokenRetriver refreshTokenRetriver)
        {
            _authService = authService;
            _tokenService = tokenService;
            _userService = userService;
            _RefreshTokenService = refreshTokenService;
            _RefreshTokenRetriver = refreshTokenRetriver;

        }

        [HttpPost("send-magic-link")]
        public async Task<ActionResult<ApiResponse<object>>> SendMagicLink([FromBody] EmailRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<object>.FailResponse("Validation failed.", errors));
            }

            var success = await _authService.SendMagicLinkAsync(model.Email);
            if (success)
                return Ok(ApiResponse<object>.SuccessResponse(null, $"Sign-up link sent to {model.Email}"));

            return BadRequest(ApiResponse<object>.FailResponse("Could not send link."));
        }

        [HttpGet("confirm-signup")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> ConfirmSignUp([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid confirmation link."));

            var response = await _authService.RegisterUserFromMagicLinkAsync(userId, token);
            if (response != null)
                return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "Registration confirmed. Proceed to plan selection."));

            return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Invalid or expired token."));
        }

        [HttpPost("register-with-password")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RegisterWithPassword([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Validation failed.", errors));
            }

            var response = await _authService.RegisterWithPasswordAsync(model);
            if (response != null)
                return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "Account created."));

            return BadRequest(ApiResponse<LoginResponseDto>.FailResponse("Registration failed."));
        }




        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var userResult = await _userService.LoginAsync(request);
            if (!userResult.Success)
            {
                return Unauthorized("Invalid credentials");
            }


            var userData = await GetClims(request.Email);
            var claims = userData.Item1;
            RegisterDto user = userData.Item2;
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

            return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
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

        async Task<(Claim[], RegisterDto)> GetClims(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, "User")
            };

            return (claims, user);
        }

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
    }
}

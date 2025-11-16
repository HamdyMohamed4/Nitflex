using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;


namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
    }
}

using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // POST: api/User/transfer-profile
        // Transfers profile data from the logged-in user's account to an existing target account.
        [HttpPost("transfer-profile")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ApiResponse<object>>> TransferProfile([FromBody] TransferProfileRequest request)
        {
            if (request == null || request.TargetUserId == Guid.Empty)
                return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));

            try
            {
                // Get caller id from claims (UserService will also validate ownership)
                var callerIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(callerIdClaim))
                    return Unauthorized(ApiResponse<object>.FailResponse("User is not authenticated."));

                var sourceUserId = Guid.Parse(callerIdClaim);

                var result = await _userService.TransferProfileAsync(sourceUserId, request.TargetUserId);

                if (!result.Success)
                {
                    _logger.LogWarning("Profile transfer failed. Source: {SourceId} Target: {TargetId} Reason: {Reason}",
                        sourceUserId, request.TargetUserId, result.Message);

                    // return appropriate status codes for common failure types
                    if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                        return NotFound(ApiResponse<object>.FailResponse(result.Message));

                    if (result.Message.Contains("already has profile", StringComparison.OrdinalIgnoreCase) ||
                        result.Message.Contains("already has data", StringComparison.OrdinalIgnoreCase))
                        return Conflict(ApiResponse<object>.FailResponse(result.Message));

                    if (result.Message.Contains("not owner", StringComparison.OrdinalIgnoreCase) ||
                        result.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase))
                        return Forbid();

                    return BadRequest(ApiResponse<object>.FailResponse(result.Message));
                }

                _logger.LogInformation("Profile transferred successfully. Source: {SourceId} Target: {TargetId}",
                    sourceUserId, request.TargetUserId);

                return Ok(ApiResponse<object>.SuccessResponse(null, "Profile transferred successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while transferring profile from {SourceId} to {TargetId}", 
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, request?.TargetUserId);
                return StatusCode(500, ApiResponse<object>.FailResponse("Internal server error", new List<string> { ex.Message }));
            }
        }

        // POST: api/User/transfer-profile-by-email
        // Transfers profile data from the logged-in user's account to a target account identified by email.
        [HttpPost("transfer-profile-by-email")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ApiResponse<object>>> TransferProfileByEmail([FromBody] TransferProfileByEmailRequest request)
        {
            if (request == null || request.ProfileId == Guid.Empty || string.IsNullOrEmpty(request.Email))
                return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));

            try
            {
                // Get caller id from claims (UserService will also validate ownership)
                var callerIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(callerIdClaim))
                    return Unauthorized(ApiResponse<object>.FailResponse("User is not authenticated."));

                var sourceUserId = Guid.Parse(callerIdClaim);

                var result = await _userService.TransferProfileByEmailAsync(sourceUserId, request.ProfileId, request.Email);

                if (!result.Success)
                {
                    _logger.LogWarning("Profile transfer by email failed. Source: {SourceId} Target Email: {TargetEmail} Reason: {Reason}",
                        sourceUserId, request.Email, result.Message);

                    // return appropriate status codes for common failure types
                    if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                        return NotFound(ApiResponse<object>.FailResponse(result.Message));

                    if (result.Message.Contains("already has profile", StringComparison.OrdinalIgnoreCase) ||
                        result.Message.Contains("already has data", StringComparison.OrdinalIgnoreCase))
                        return Conflict(ApiResponse<object>.FailResponse(result.Message));

                    if (result.Message.Contains("not owner", StringComparison.OrdinalIgnoreCase) ||
                        result.Message.Contains("not authorized", StringComparison.OrdinalIgnoreCase))
                        return Forbid();

                    return BadRequest(ApiResponse<object>.FailResponse(result.Message));
                }

                _logger.LogInformation("Profile transferred by email successfully. Source: {SourceId} Target Email: {TargetEmail}",
                    sourceUserId, request.Email);

                return Ok(ApiResponse<object>.SuccessResponse(null, "Profile transferred by email successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while transferring profile by email from {SourceId} to {TargetEmail}", 
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, request?.Email);
                return StatusCode(500, ApiResponse<object>.FailResponse("Internal server error", new List<string> { ex.Message }));
            }
        }
    }
}

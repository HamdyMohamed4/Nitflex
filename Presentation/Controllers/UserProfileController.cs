using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly ITransferService _transferService;
        private readonly IUserService _userService;
        private readonly ILogger<UserProfileController> _logger;
        private readonly IConfiguration _configuration;

        public UserProfileController(ITransferService transferService, IUserService userService, ILogger<UserProfileController> logger, IConfiguration configuration)
        {
            _transferService = transferService;
            _userService = userService;
            _logger = logger;
            _configuration = configuration;
        }

        // POST: api/UserProfile/transfer
        // Transfers an existing profile to another user (identified by email).
        [HttpPost("transfer")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ApiResponse<object>>> InitiateTransfer([FromBody] InitiateProfileTransferRequest request)
        {
            if (request == null || request.ProfileId == Guid.Empty || string.IsNullOrWhiteSpace(request.TargetEmail))
                return BadRequest(ApiResponse<object>.FailResponse("ProfileId and TargetEmail are required."));

            Guid callerId;
            try { callerId = _userService.GetLoggedInUser(); }
            catch { return Unauthorized(ApiResponse<object>.FailResponse("User not authenticated.")); }

            // frontend confirmation URL base - read from config or accept param
            var frontendBase = _configuration["Frontend:TransferConfirmUrl"] ?? _configuration["Frontend:BaseUrl"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(frontendBase))
                return StatusCode(500, ApiResponse<object>.FailResponse("Server not configured for transfer confirmation links."));

            var (success, message) = await _transferService.InitiateTransferAsync(callerId, request, frontendBase);

            if (!success)
                return BadRequest(ApiResponse<object>.FailResponse(message));

            _logger.LogInformation("User {UserId} initiated profile transfer for profile {ProfileId} to {Email}", callerId, request.ProfileId, request.TargetEmail);
            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        // POST: api/UserProfile/transfer/confirm
        // Receiver clicks link and frontend calls this endpoint with token and approve flag.
        [HttpPost("transfer/confirm")]
        public async Task<ActionResult<ApiResponse<object>>> ConfirmTransfer([FromBody] ConfirmTransferRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Token))
                return BadRequest(ApiResponse<object>.FailResponse("Token is required."));

            var (success, message) = await _transferService.CompleteTransferAsync(request);

            if (!success)
                return BadRequest(ApiResponse<object>.FailResponse(message));

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }
    }
}

using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubsciptionService _subscriptionService;
        private readonly IUserService _userService;

        public SubscriptionController(ISubsciptionService subscriptionService, IUserService userService)
        {
            _subscriptionService = subscriptionService;
            _userService = userService;
        }

        // *************************************
        // ************ Admin Endpoints ********
        // *************************************

        [HttpGet("plans")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<SubscriptionPlanDto>>>> GetPlans()
        {
            var plans = await _subscriptionService.GetPlansAsync();

            return Ok(ApiResponse<List<SubscriptionPlanDto>>.SuccessResponse(plans, "Plans retrieved successfully"));
        }

        [HttpGet("plans/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<SubscriptionPlanDto?>>> GetPlan(Guid id)
        {
            var plan = await _subscriptionService.GetPlanByIdAsync(id);

            if (plan == null)
                return NotFound(ApiResponse<SubscriptionPlanDto?>.FailResponse("Plan not found"));

            return Ok(ApiResponse<SubscriptionPlanDto?>.SuccessResponse(plan));
        }

        [HttpPost("plans")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<SubscriptionPlanDto>>> CreatePlan(CreateSubscriptionPlanDto dto)
        {
            var plan = await _subscriptionService.CreatePlanAsync(dto);

            return Ok(ApiResponse<SubscriptionPlanDto>.SuccessResponse(plan, "Plan created successfully"));
        }

        [HttpPut("plans/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<SubscriptionPlanDto?>>> UpdatePlan(Guid id, UpdateSubscriptionPlanDto dto)
        {
            var updated = await _subscriptionService.UpdatePlanAsync(id, dto);

            if (updated == null)
                return NotFound(ApiResponse<SubscriptionPlanDto?>.FailResponse("Plan not found"));

            return Ok(ApiResponse<SubscriptionPlanDto?>.SuccessResponse(updated, "Plan updated successfully"));
        }

        [HttpDelete("plans/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePlan(Guid id)
        {
            var deleted = await _subscriptionService.DeletePlanAsync(id);

            if (!deleted)
                return NotFound(ApiResponse<bool>.FailResponse("Plan not found"));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Plan deleted successfully"));
        }


        // *************************************
        // ************ User Endpoints *********
        // *************************************

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserSubscriptionDto?>>> GetMySubscription()
        {
            var userId = _userService.GetLoggedInUser().ToString();

            if (userId == null)
                return Unauthorized(ApiResponse<UserSubscriptionDto?>.FailResponse("User not logged in"));

            var subscription = await _subscriptionService.GetCurrentUserSubscriptionAsync(userId);

            return Ok(ApiResponse<UserSubscriptionDto?>.SuccessResponse(subscription, "Current subscription retrieved"));
        }

        [HttpPost("subscribe")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserSubscriptionDto>>> Subscribe(CreateUserSubscriptionDto dto)
        {
            var userId = _userService.GetLoggedInUser().ToString();

            if (userId == null)
                return Unauthorized(ApiResponse<UserSubscriptionDto>.FailResponse("User not logged in"));

            var result = await _subscriptionService.SubscribeAsync(userId, dto);

            return Ok(ApiResponse<UserSubscriptionDto>.SuccessResponse(result, "Subscription activated successfully"));
        }
    }
}

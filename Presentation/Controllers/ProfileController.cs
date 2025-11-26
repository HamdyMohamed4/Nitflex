using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using System.Security.Claims;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    [Route("AllProfiles")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse<List<UserProfileDto>>>> GetAllProfilesByUserIdAsync(Guid userId)
    {
        var response = await _profileService.GetAllProfilesByUserIdAsync(userId);

        if (!response.Status)
        {
            if (response.Message.Contains("Not Found"))
            {
                return NotFound(ApiResponse<List<UserProfileDto>>.FailResponse(response.Message));
            }

            else
            {
                return BadRequest(ApiResponse<List<UserProfileDto>>.FailResponse("Something went wrong"));
            }
        }

        return Ok(ApiResponse<IEnumerable<UserProfileDto>>.SuccessResponse(response.Response, response.Message));
    }

    [HttpGet]
    [Authorize(Roles = "User")]
    [Route("GetProfile/{profileId}")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfileByProfileAsync(Guid profileId)
    {
        // 🔥 Get logged in user ID from Token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<CreateProfileDto>.FailResponse("User not authenticated."));

        var response = await _profileService.GetProfileByUserIdAsync(Guid.Parse(userId), profileId);

        if (!response.Status)
        {
            if (response.Message.Contains("Not Found"))
            {
                return NotFound(ApiResponse<UserProfileDto>.FailResponse(response.Message));
            }

            else
            {
                return BadRequest(ApiResponse<UserProfileDto>.FailResponse("Soemthing went wrong"));
            }
        }

        return Ok(ApiResponse<UserProfileDto>.SuccessResponse(response.userProfileDto!, response.Message));
    }

    [HttpGet]
    [Route("GetViewHistory/{profileId}")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserHistoryDto>>>> GetViewingHistory(Guid profileId)
    {

        // 🔥 Get logged in user ID from Token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<CreateProfileDto>.FailResponse("User not authenticated."));
        var response = await _profileService.GetViewingHistoryAsync(Guid.Parse(userId), profileId);

        if (!response.Status)
        {
            if (response.Message.Contains("Not Found"))
                return NotFound(ApiResponse<IEnumerable<UserHistoryDto>>.FailResponse(response.Message));

            return BadRequest(ApiResponse<IEnumerable<UserHistoryDto>>.FailResponse(response.Message));
        }

        else
        {
            return Ok(ApiResponse<IEnumerable<UserHistoryDto>>.SuccessResponse(response.userHistoryListDto, response.Message));
        }
    }

    [HttpPost]
    [Route("CreateProfile")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse<CreateProfileDto>>> CreateProfileForUserAsync([FromBody] CreateProfileDto createProfileDto)
    {
        // 🔥 Get logged in user ID from Token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<CreateProfileDto>.FailResponse("User not authenticated."));


        var response = await _profileService.CreateProfileAsync(Guid.Parse(userId), createProfileDto);

        if (!response.Status)
        {
            if (response.Message.Contains("Not Found"))
                return NotFound(ApiResponse<CreateProfileDto>.FailResponse(response.Message));

            return BadRequest(ApiResponse<CreateProfileDto>.FailResponse(response.Message));
        }

        return Ok(ApiResponse<CreateProfileDto>.SuccessResponse(response.userProfileDto, response.Message));
    }


    //[HttpPost]
    //[Route("Update/{}")]

    //[HttpDelete]
    //[Route("Delete/{profileId}")]
    //[Authorize(Roles = "User")]
    //public async Task<ActionResult<ApiResponse<bool>>> DeleteProfile(Guid profileId)
    //{

    //    var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
    //    var response = await _profileService.DeleteProfileByUserId(profileId, userId);

    //    if (!response.Status)
    //    {
    //        if (response.Message.Contains("Not Found"))
    //            return NotFound(ApiResponse<bool>.FailResponse(response.Message));

    //        return BadRequest(ApiResponse<bool>.FailResponse(response.Message));
    //    }

    //    else
    //    {
    //        return Ok(ApiResponse<bool>.SuccessResponse(true, response.Message));
    //    }
    //}

    [HttpDelete]
    [Route("Delete/{profileId}")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProfile(Guid profileId)
    {
        // 🔥 جلب userId من الـ token مع التحقق
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiResponse<bool>.FailResponse("User not authenticated."));

        var response = await _profileService.DeleteProfileByUserId(profileId, userId);

        if (!response.Status)
        {
            if (response.Message.Contains("Not Found"))
                return NotFound(ApiResponse<bool>.FailResponse(response.Message));

            return BadRequest(ApiResponse<bool>.FailResponse(response.Message));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, response.Message));
    }



}
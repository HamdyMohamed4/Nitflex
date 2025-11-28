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

    private Guid? GetUserIdFromClaims()
    {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(id, out var g)) return g;
            return null;
    }

    [HttpGet]
    [Route("AllProfiles")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse<List<UserProfileDto>>>> GetAllProfilesByUserIdAsync()
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
            return Unauthorized(ApiResponse<List<UserProfileDto>>.FailResponse("User not authenticated."));

        try
        {
            var profiles = await _profileService.GetAllProfilesAsync(userId.Value);
            return Ok(ApiResponse<List<UserProfileDto>>.SuccessResponse(profiles, "Profiles retrieved"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<UserProfileDto>>.FailResponse(ex.Message));
        }
    }

    [HttpGet]
    [Authorize(Roles = "User")]
    [Route("GetProfile/{profileId}")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfileByProfileAsync(Guid profileId)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
            return Unauthorized(ApiResponse<UserProfileDto>.FailResponse("User not authenticated."));

        var profile = await _profileService.GetProfileByIdAsync(profileId, userId.Value);
        if (profile == null)
            return NotFound(ApiResponse<UserProfileDto>.FailResponse("Profile not found."));

        return Ok(ApiResponse<UserProfileDto>.SuccessResponse(profile, "Profile retrieved"));
    }

    [HttpGet]
    [Route("GetViewHistory/{profileId}")]
    [Authorize(Roles = "User")]
    public Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetViewingHistory(Guid profileId)
    {
        // This method previously relied on a profile service method that isn't part of the simplified IProfileService.
        // Return 501 Not Implemented so callers know to use the dedicated history endpoint.
        return Task.FromResult<ActionResult<ApiResponse<IEnumerable<object>>>>(StatusCode(501, ApiResponse<IEnumerable<object>>.FailResponse("Viewing history endpoint not implemented here. Use /api/UserHistory endpoints.")));
    }

    [HttpPost]
    [Route("CreateProfile")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse<CreateProfileDto>>> CreateProfileForUserAsync([FromBody] CreateProfileDto createProfileDto)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
            return Unauthorized(ApiResponse<CreateProfileDto>.FailResponse("User not authenticated."));

        try
        {
            var created = await _profileService.CreateProfileAsync(createProfileDto, userId.Value);
            return Ok(ApiResponse<CreateProfileDto>.SuccessResponse(createProfileDto, "Profile created"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CreateProfileDto>.FailResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CreateProfileDto>.FailResponse("Internal server error", new List<string> { ex.Message }));
        }
    }

    [HttpDelete]
    [Route("Delete/{profileId}")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProfile(Guid profileId)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
            return Unauthorized(ApiResponse<bool>.FailResponse("User not authenticated."));

        var deleted = await _profileService.DeleteProfileAsync(profileId, userId.Value);
        if (!deleted)
            return NotFound(ApiResponse<bool>.FailResponse("Profile not found or not owned by user."));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Profile deleted"));
    }
}
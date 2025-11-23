using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Microsoft.AspNetCore.Mvc;
using Presentation.ExtensionMethods;
using Presentation.Models;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IFileService _fileService;

    public ProfileController(IProfileService profileService, IFileService fileService)
    {
        _profileService = profileService;
        _fileService = fileService;
    }

    [HttpGet]
    [Route("AllProfiles/{userId}")]
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
    [Route("GetProfileByUserId/{userId}/{profileId}")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfileByUserIdAsync(Guid userId, Guid profileId)
    {
        var response = await _profileService.GetProfileByUserIdAsync(userId, profileId);

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
    [Route("GetViewHistory/{userId}/{profileId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserHistoryDto>>>> GetViewingHistory(Guid userId, Guid profileId)
    {
        var response = await _profileService.GetViewingHistoryAsync(userId, profileId);

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
    [Route("CreateProfileByUserId/{userId}")]
    public async Task<ActionResult<ApiResponse<CreateProfileDto>>> CreateProfileForUserWithIdAsync(Guid userId, [FromBody] CreateProfileDto createProfileDto)
    {
        var response = await _profileService.CreateProfileAsync(userId, createProfileDto);

        if (!response.Status)
        {
            if (response.Message.Contains("Not Found"))
                return NotFound(ApiResponse<CreateProfileDto>.FailResponse(response.Message));

            return BadRequest(ApiResponse<CreateProfileDto>.FailResponse(response.Message));
        }

        return Ok(ApiResponse<CreateProfileDto>.SuccessResponse(response.userProfileDto, response.Message));
    }

    // [HttpPost]
    // [Route("Update/{}")]

    [HttpDelete]
    [Route("Delete/{userId}/{profileId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProfile(Guid userId, Guid profileId)
    {
        var response = await _profileService.DeleteProfileByUserId(userId, profileId);

        if (!response.Status)
        {
            if (response.Message.Contains("Not Found"))
                return NotFound(ApiResponse<bool>.FailResponse(response.Message));

            return BadRequest(ApiResponse<bool>.FailResponse(response.Message));
        }

        else
        {
            return Ok(ApiResponse<bool>.SuccessResponse(true, response.Message));
        }
    }

    [HttpPost]
    [Route("UploadProfilePicture/{userId}/{profileId}")]
    public async Task<ActionResult<ApiResponse<bool>>> UploadProfilePicture(Guid userId, Guid profileId, IFormFile file)
    {
        FileUploadDto fileUploadDto = file.ToFileUploadDto();
        var response = await _profileService.UploadProfilePicture(userId, profileId, fileUploadDto);

        if (!response.Status)
        {
            if (response.Message.Contains("Not Found"))
                return NotFound(ApiResponse<bool>.FailResponse(response.Message));

            return BadRequest(ApiResponse<bool>.FailResponse(response.Message));
        }

        else
        {
            return Ok(ApiResponse<bool>.SuccessResponse(true, response.Message));
        }
    }

    [HttpPost]
    [Route("DeleteProfilePicture/{userId}/{profileId}/{profilePicturePath}")]
    public async Task<IActionResult> DeleteProfilePicture(Guid userId, Guid profileId, string profilePicturePath)
    {
        var response = await _profileService.DeleteProfilePicture(userId, profileId, profilePicturePath);

        if (!response.Status)
        {
            if (response.Message.Contains("Not Found"))
                return NotFound(ApiResponse<bool>.FailResponse(response.Message));

            return BadRequest(ApiResponse<bool>.FailResponse(response.Message));
        }

        else
        {
            return Ok(ApiResponse<bool>.SuccessResponse(true, response.Message));
        }
    }
}
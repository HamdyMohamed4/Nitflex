using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfilesController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfilesController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    private Guid? GetCurrentUserId()
    {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(id, out var g)) return g;
            return null;
    }

    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { Message = "User not authenticated" });

        try
        {
            var created = await _profileService.CreateProfileAsync(dto, userId.Value);
            return Ok(created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Internal server error", Details = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { Message = "User not authenticated" });

        var list = await _profileService.GetAllProfilesAsync(userId.Value);
        return Ok(list);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { Message = "User not authenticated" });

        var profile = await _profileService.GetProfileByIdAsync(id, userId.Value);
        if (profile == null) return NotFound(new { Message = "Profile not found" });
        return Ok(profile);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { Message = "User not authenticated" });

        var success = await _profileService.DeleteProfileAsync(id, userId.Value);
        if (!success) return NotFound(new { Message = "Profile not found or not owned by user" });
        return NoContent();
    }
}

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

    [HttpPost("{id}/lock")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> LockProfile(Guid id, [FromBody] LockRequest req)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { Message = "User not authenticated" });

        if (req == null || string.IsNullOrEmpty(req.PinCode)) return BadRequest(new { Message = "Pin code is required" });
        if (req.PinCode.Length != 4 || !req.PinCode.All(char.IsDigit)) return BadRequest(new { Message = "PIN must be 4 numeric digits" });

        try
        {
            var result = await _profileService.LockProfileAsync(id, userId.Value, req.PinCode);
            if (!result) return NotFound(new { Message = "Profile not found or not owned by user" });
            return Ok(new { Message = "Profile locked" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{id}/unlock")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> UnlockProfile(Guid id, [FromBody] LockRequest req)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { Message = "User not authenticated" });

        if (req == null || string.IsNullOrEmpty(req.PinCode)) return BadRequest(new { Message = "Pin code is required" });
        if (req.PinCode.Length != 4 || !req.PinCode.All(char.IsDigit)) return BadRequest(new { Message = "PIN must be 4 numeric digits" });

        try
        {
            var ok = await _profileService.UnlockProfileAsync(id, userId.Value, req.PinCode);
            if (!ok) return BadRequest(new { Message = "Invalid PIN or profile not found" });
            return Ok(new { Message = "Profile unlocked" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPatch("{id}/kid-mode")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> ToggleKidMode(Guid id, [FromBody] KidModeRequest req)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized(new { Message = "User not authenticated" });

        try
        {
            var ok = await _profileService.ToggleKidModeAsync(id, userId.Value, req.Enable);
            if (!ok) return NotFound(new { Message = "Profile not found or not owned by user" });
            return Ok(new { Message = "Kid mode updated" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
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

    // DTOs for requests
    public class LockRequest { public string PinCode { get; set; } = string.Empty; }
    public class KidModeRequest { public bool Enable { get; set; } }
}

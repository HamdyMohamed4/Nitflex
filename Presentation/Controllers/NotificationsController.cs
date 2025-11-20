using ApplicationLayer.Dtos;
using ApplicationLayer.Services;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationsController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // إرسال إشعار إلى جميع بروفايلات المستخدم
    [HttpPost("send-to-user-profiles")]
    public async Task<ActionResult<ApiResponse<string>>> SendNotificationToUserProfiles([FromBody] NotificationDto request)
    {
        try
        {
            await _notificationService.SendNotificationToUser(request);
            return Ok(ApiResponse<string>.SuccessResponse("Notification sent to all profiles.", "Notification sent successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.FailResponse("Failed to send notification to user profiles", new List<string> { ex.Message }));
        }
    }

    // إرسال إشعار إلى جميع المستخدمين
    [HttpPost("send-to-all")]
    public async Task<ActionResult<ApiResponse<string>>> SendNotificationToAll([FromBody] NotificationDto request)
    {
        try
        {
            await _notificationService.SendNotificationToAll(request.Message, request.Type, request.RedirectUrl);
            return Ok(ApiResponse<string>.SuccessResponse("Notification sent to all users.", "Notification sent successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.FailResponse("Failed to send notification to all users", new List<string> { ex.Message }));
        }
    }
}

using ApplicationLayer.Dtos;
using ApplicationLayer.Services;
using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub
{
    private readonly NotificationService _notificationService;

    public NotificationHub(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // إرسال إشعار إلى مستخدم معين عبر SignalR
    public async Task SendNotificationToUser(NotificationDto notification)
    {
        await _notificationService.SendNotificationToUser(notification);
    }

    // إرسال إشعار إلى جميع المستخدمين
    public async Task SendNotificationToAll(string message, string type, string redirectUrl = "")
    {
        await _notificationService.SendNotificationToAll(message, type, redirectUrl);
    }
}

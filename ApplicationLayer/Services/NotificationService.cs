using Microsoft.AspNetCore.SignalR;
using ApplicationLayer.Dtos;
using Domains;
using InfrastructureLayer.Contracts;
using AutoMapper;

namespace ApplicationLayer.Services
{
    public class NotificationService
    {
        private readonly IGenericRepository<Notification> _repo;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IMapper _mapper;

        public NotificationService(IGenericRepository<Notification> repo, IHubContext<NotificationHub> hubContext, IMapper mapper)
        {
            _repo = repo;
            _hubContext = hubContext;
            _mapper = mapper;
        }

        // إرسال إشعار إلى مستخدم معين وتخزينه في قاعدة البيانات
        public async Task SendNotificationToUser(NotificationDto notification)
        {
            try
            {
                // 1. تحويل NotificationDto إلى Notification (Entity) باستخدام AutoMapper
                var notificationEntity = _mapper.Map<Notification>(notification);

                // 2. إضافة التنبيه إلى قاعدة البيانات
                await _repo.Add(notificationEntity);

                // 3. إرسال التنبيه عبر SignalR للمستخدم باستخدام ProfileId
                var notificationDto = _mapper.Map<NotificationDto>(notificationEntity);
                await _hubContext.Clients.User(notification.ProfileId.ToString()).SendAsync("ReceiveNotification", notificationDto);
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., logging)
                throw new Exception("Error sending notification", ex);
            }
        }


        // إرسال إشعار إلى جميع المستخدمين وتخزينه في قاعدة البيانات
        public async Task SendNotificationToAll(string message, string type, string redirectUrl = "")
        {
            var notification = new Notification
            {
                Message = message,
                Type = type,
                RedirectUrl = redirectUrl
            };

            // إضافة التنبيه إلى قاعدة البيانات
            await _repo.Add(notification);

            // إرسال التنبيه لجميع المستخدمين عبر SignalR
            var notificationDto = _mapper.Map<NotificationDto>(notification);
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notificationDto);
        }

        // تحديث التنبيه كـ "مقروء"
        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _repo.GetById(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _repo.Update(notification);
            }
        }
    }
}

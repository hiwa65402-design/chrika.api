using Chrika.Api.Data;
using Chrika.Api.Models;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    // Services/NotificationService.cs

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        // === گۆڕانکارییەکە لێرەدایە ===
        public async Task<Notification> CreateNotificationAsync(int userId, int triggeredByUserId, NotificationType type, int? entityId)
        {
            // 1. دروستکردنی ئۆبجێکتی نۆتیفیکەیشن
            var notification = new Notification
            {
                UserId = userId,
                TriggeredByUserId = triggeredByUserId,
                Type = type,
                EntityId = entityId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            // 2. زیادکردنی بۆ بنکەی دراوە
            _context.Notifications.Add(notification);

            // 3. === گرنگترین بەش ===
            // گەڕاندنەوەی ئۆبجێکتە نوێیەکە
            return notification;
        }

        public string GenerateNotificationMessage(NotificationType type, string username)
        {
            return type switch
            {
                NotificationType.NewLike => $"{username} liked your post.",
                NotificationType.NewComment => $"{username} commented on your post.",
                NotificationType.NewFollower => $"{username} started following you.",
                _ => "You have a new notification."
            };
        }
    }
}

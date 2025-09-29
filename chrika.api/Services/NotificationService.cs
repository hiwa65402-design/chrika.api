using Chrika.Api.Data;
using Chrika.Api.Models;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(int userId, int triggeredByUserId, NotificationType type, int? entityId)
        {
            // دڵنیابە کە کەسێک بۆ خۆی ئاگادارکردنەوە دروست ناکات
            if (userId == triggeredByUserId) return;

            var notification = new Notification
            {
                UserId = userId,
                TriggeredByUserId = triggeredByUserId,
                Type = type,
                EntityId = entityId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}

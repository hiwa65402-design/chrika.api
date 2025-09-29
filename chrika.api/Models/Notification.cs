using System.ComponentModel.DataAnnotations;

namespace Chrika.Api.Models
{
    public enum NotificationType
    {
        NewFollower,
        NewLike,
        NewComment
    }

    public class Notification
    {
        public int Id { get; set; }

        // ئەو بەکارهێنەرەی کە ئاگادارکردنەوەکەی بۆ دەچێت
        public int UserId { get; set; }
        public virtual User? User { get; set; }

        // ئەو بەکارهێنەرەی کە بووەتە هۆی دروستبوونی ئاگادارکردنەوەکە
        public int TriggeredByUserId { get; set; }
        public virtual User? TriggeredByUser { get; set; }

        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        // IDی ئەو شتەی کە پەیوەندی بە ئاگادارکردنەوەکەوە هەیە (بۆ نموونە، PostId)
        public int? EntityId { get; set; }
    }
}

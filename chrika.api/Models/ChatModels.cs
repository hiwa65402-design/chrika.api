// Models/ChatModels.cs
using System;
using System.Collections.Generic;

namespace Chrika.Api.Models
{
    // Enum بۆ دیاریکردنی جۆری نامە
    public enum MessageType
    {
        Text,
        Image,
        Video,
        Audio, // دەنگی تۆمارکراو
        Forwarded // نامەیەکی تری forward کراو
    }

    // نوێنەرایەتی گفتوگۆیەکی نێوان دوو بەکارهێنەر دەکات
    public class Conversation
    {
        public int Id { get; set; }

        public int Participant1Id { get; set; }
        public virtual User? Participant1 { get; set; }

        public int Participant2Id { get; set; }
        public virtual User? Participant2 { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastMessageAt { get; set; }

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }

    // نوێنەرایەتی تاکە نامەیەک دەکات (وەشانی نوێ)
    public class Message
    {
        public int Id { get; set; }
        public MessageType Type { get; set; } = MessageType.Text;

        // بۆ نامەی جۆری Text
        public string? Content { get; set; }

        // بۆ نامەی جۆری Image, Video, Audio
        public string? MediaUrl { get; set; }
        public double? MediaDuration { get; set; } // بۆ دەنگ و ڤیدیۆ (بە چرکە)

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public bool IsDeleted { get; set; } = false; // بۆ سڕینەوەی نامە

        // --- پەیوەندییەکان ---
        public int ConversationId { get; set; }
        public virtual Conversation? Conversation { get; set; }

        public int SenderId { get; set; }
        public virtual User? Sender { get; set; }

        // بۆ نامەی جۆری Forwarded

        public int? ForwardedMessageId { get; set; }
        public virtual Message? ForwardedMessage { get; set; }
    }

}

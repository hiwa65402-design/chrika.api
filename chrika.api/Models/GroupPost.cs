using System;

namespace Chrika.Api.Models
{
    // ======================================================
    // === Enum بۆ دیاریکردنی جۆری پۆستی گرووپ ===
    // ======================================================
    public enum GroupPostType
    {
        Text,       // تەنها نووسین
        Image,      // نووسین + وێنە
        Video,      // نووسین + ڤیدیۆ
        LiveStream  // لایڤ ستریم
    }

    // ======================================================
    // === کڵاسی سەرەکی بۆ پۆستی ناو گرووپ ===
    // ======================================================
    public class GroupPost
    {
        public int Id { get; set; }
        public string? Content { get; set; } // دەتوانێت null بێت ئەگەر تەنها وێنە/ڤیدیۆ بێت

        // --- زانیاری میدیا ---
        public GroupPostType PostType { get; set; } = GroupPostType.Text; // جۆری پۆست
        public string? MediaUrl { get; set; } // URLی وێنە یان ڤیدیۆ

        // --- زانیاری تایبەت بە لایڤ ستریم ---
        public bool IsLive { get; set; } = false; // ئایا لایڤەکە ئێستا چالاکە؟
        public string? LiveStreamUrl { get; set; } // URLی سێرڤەری لایڤ (بۆ نموونە RTMP)
        public string? PlaybackUrl { get; set; } // URLی بینینی لایڤەکە (بۆ نموونە HLS/DASH)
        public DateTime? LiveEndedAt { get; set; } // کاتی کۆتاییهاتنی لایڤ

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- پەیوەندییەکان ---
        public int AuthorId { get; set; }
        public virtual User? Author { get; set; }

        public int GroupId { get; set; }
        public virtual Group? Group { get; set; }
    }
}

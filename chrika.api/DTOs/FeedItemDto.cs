using System;

namespace Chrika.Api.DTOs
{
    /// <summary>
    /// ئەم DTOـیە بۆ ئەوەیە کە بتوانین هەم پۆستی ئاسایی و هەم پۆستی گرووپ
    /// لە یەک لیستدا (Feed) پیشان بدەین.
    /// </summary>
    public class FeedItemDto
    {
        // --- زانیاری گشتی کە لە هەردوو جۆردا هەیە ---
        public int Id { get; set; }
        public string? ItemType { get; set; } // "Post" یان "GroupPost"
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }

        // --- زانیاری نووسەر ---
        public int AuthorId { get; set; }
        public string? AuthorUsername { get; set; }
        public string? AuthorProfilePicture { get; set; }

        // --- زانیاری تایبەت بە پۆستی گرووپ (بۆ پۆستی ئاسایی null دەبێت) ---
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? GroupUsername { get; set; }

        // --- زانیاری میدیا ---
        public string? PostType { get; set; } // "Text", "Image", "Video"
        public string? MediaUrl { get; set; }

        // --- ئامارەکان ---
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public bool IsSponsored { get; set; } = false;
        public int? AdCampaignId { get; set; }
    }
}

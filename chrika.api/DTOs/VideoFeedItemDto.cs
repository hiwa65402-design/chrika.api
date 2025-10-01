// DTOs/VideoFeedItemDto.cs

using System;

namespace Chrika.Api.DTOs
{
    /// <summary>
    /// ئەم DTOـیە تەنها بۆ بەشی ڤیدیۆ بەکاردێت (هاوشێوەی TikTok).
    /// </summary>
    public class VideoFeedItemDto
    {
        // زانیاری سەرەکی
        public int Id { get; set; }
        public string? ItemType { get; set; } // "Post" or "GroupPost"
        public string? Content { get; set; } // Captionـی ڤیدیۆکە
        public string? MediaUrl { get; set; } // URLـی ڤیدیۆکە (نابێت null بێت)
        public DateTime CreatedAt { get; set; }

        // زانیاری نووسەر
        public int AuthorId { get; set; }
        public string? AuthorUsername { get; set; }
        public string? AuthorProfilePicture { get; set; }

        // زانیاری گرووپ (ئەگەر هی گرووپ بوو)
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }

        // ئامارەکان
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
    }
}

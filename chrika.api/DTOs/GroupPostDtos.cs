using Chrika.Api.Models; // بۆ بەکارهێنانی GroupPostType
using System.ComponentModel.DataAnnotations;

namespace Chrika.Api.DTOs
{
    // DTO بۆ دروستکردنی پۆستی نوێ لەناو گرووپ
    public class CreateGroupPostDto
    {
        public string? Content { get; set; }

        [Required]
        public GroupPostType PostType { get; set; } = GroupPostType.Text;

        // URLی میدیاکە (وێنە یان ڤیدیۆ)
        // دواتر دەتوانین سیستەمی uploadی بۆ دروست بکەین
        public string? MediaUrl { get; set; }
    }

    // DTO بۆ پیشاندانی پۆستێکی گرووپ
    public class GroupPostDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? PostType { get; set; }
        public string? MediaUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        // زانیاری نووسەر
        public int AuthorId { get; set; }
        public string? AuthorUsername { get; set; }
        public string? AuthorProfilePicture { get; set; }

        // ئامارەکان (بۆ دواتر)
        public int LikesCount { get; set; } = 0;
        public int CommentsCount { get; set; } = 0;
    }
}

namespace Chrika.Api.DTOs
{
    // بۆ پیشاندانی زانیاری کۆمێنتێک
    public class CommentDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Username { get; set; } // ناوی ئەو کەسەی کۆمێنتەکەی نووسیوە
        public string? UserProfilePicture { get; set; }

    }

    // بۆ وەرگرتنی زانیاری لە کاتی دروستکردنی کۆمێنتی نوێ
    public class CreateCommentDto
    {
        public string? Content { get; set; }
    }
}

namespace Chrika.Api.DTOs
{
    // ئەمە بۆ پیشاندانی زانیاری پۆستێکە
    public class PostDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Username { get; set; } // ناوی خاوەنی پۆستەکە
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
    }

    // ئەمە بۆ وەرگرتنی زانیاری لە کاتی دروستکردنی پۆستی نوێ
    public class CreatePostDto
    {
        public string? Content { get; set; }
        public string? ImageUrl { get; set; } // بۆ ئێستا وەک string وەریدەگرین
    }

    // ئەمە بۆ وەرگرتنی زانیاری لە کاتی نوێکردنەوەی پۆست
    public class UpdatePostDto
    {
        public string? Content { get; set; }
    }
}

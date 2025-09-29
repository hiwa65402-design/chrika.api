namespace Chrika.Api.Dtos
{
    // ئەمە بۆ پیشاندانی زانیاری پۆستێکە
    public class PostDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        // --- زانیاری خاوەنی پۆست ---
        public int UserId { get; set; } 
        public string? Username { get; set; }
        public string? UserProfilePicture { get; set; } 

        // --- ئامارەکان ---
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }

        // --- زانیاری بۆ بەکارهێنەری ئێستا ---
        public bool IsLikedByCurrentUser { get; set; } 

        // --- ئاڵای سپۆنسەر ---
        public bool IsSponsoredPost { get; set; } = false;
    }

    // ئەمە بۆ وەرگرتنی زانیاری لە کاتی دروستکردنی پۆستی نوێ
    public class CreatePostDto
    {
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
    }

    // ئەمە بۆ وەرگرتنی زانیاری لە کاتی نوێکردنەوەی پۆست
    public class UpdatePostDto
    {
        public string? Content { get; set; }
    }
}

namespace Chrika.Api.Dtos
{
    // ئەم کڵاسە گشتییە بۆ ئەوەیە بزانین ئەنجامەکە بەکارهێنەرە یان پۆست
    public class SearchResultDto
    {
        public string? ResultType { get; set; } // "User" or "Post"
        public object? Data { get; set; }
    }

    // دەتوانین DTO ـی تایبەت بە گەڕانی بەکارهێنەر دروست بکەین بۆ ئەوەی زانیاری کەمتر بگەڕێنینەوە
    public class UserSearchResultDto
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? ProfilePicture { get; set; }
    }

    // بۆ پۆستیش دەتوانین DTO ـی سادەتر بەکاربهێنین
    public class PostSearchResultDto
    {
        public int Id { get; set; }
        public string? ContentSnippet { get; set; } // پارچەیەکی کورت لە ناوەڕۆکی پۆستەکە
        public string? AuthorUsername { get; set; }
    }
}

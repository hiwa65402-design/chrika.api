namespace Chrika.Api.DTOs
{
    // DTO بۆ دروستکردنی پەیجی نوێ
    public class CreatePageDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    // DTO بۆ پیشاندانی زانیاری پەیج
    public class PageDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ProfilePicture { get; set; }
        public string? CoverPicture { get; set; }
        public string? OwnerUsername { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // DTO بۆ دروستکردنی پۆستی پەیج
    public class CreatePagePostDto
    {
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
    }

    // DTO بۆ پیشاندانی پۆستی پەیج
    public class PagePostDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PageId { get; set; }
        public bool IsSponsored { get; set; }
        public DateTime? SponsoredUntil { get; set; }
        public string? TargetLocation { get; set; }
    }
}

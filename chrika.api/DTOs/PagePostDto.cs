using System.ComponentModel.DataAnnotations;

namespace Chrika.Api.Dtos
{
    // بۆ دروستکردنی پۆستێکی نوێی پەیج
    public class CreatePagePostDto
    {
        [Required]
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
    }

    // بۆ پیشاندانی زانیاری پۆستی پەیج
    public class PagePostDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PageId { get; set; }

        // زانیارییەکانی سپۆنسەر
        public bool IsSponsored { get; set; }
        public DateTime? SponsoredUntil { get; set; }
        public string? TargetLocation { get; set; }
    }
}

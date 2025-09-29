namespace Chrika.Api.Models
{
    public class PagePost
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // پەیوەندی: ئەم پۆستە هی کام پەیجەیە
        public int PageId { get; set; }
        public virtual Page Page { get; set; }

        // === تایبەتمەندییەکانی سپۆنسەرکردن لێرەدان ===
        public bool IsSponsored { get; set; } = false;
        public DateTime? SponsoredUntil { get; set; }
        public string? TargetLocation { get; set; }
    }
}

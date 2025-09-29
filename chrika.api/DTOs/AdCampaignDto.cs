using System.ComponentModel.DataAnnotations;

namespace Chrika.Api.Dtos
{
    // DTO بۆ ئامانجی بینەر
    public class TargetAudienceDto
    {
        public List<string> Locations { get; set; } = new();
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public string? Gender { get; set; }
    }

    // DTO بۆ دروستکردنی کەمپەینێکی نوێ
    public class CreateAdCampaignDto
    {
        [Required]
        public int PagePostId { get; set; }

        [Required]
        [Range(1, double.MaxValue)]
        public decimal Budget { get; set; }

        public string Currency { get; set; } = "USD";

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public TargetAudienceDto? Audience { get; set; }
    }

    // DTO بۆ پیشاندانی وردەکاری کەمپەین
    public class AdCampaignDto
    {
        public int Id { get; set; }
        public int PagePostId { get; set; }
        public string? Status { get; set; }
        public decimal Budget { get; set; }
        public string? Currency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TargetAudienceDto? Audience { get; set; }
    }
}

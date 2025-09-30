using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chrika.Api.Models
{
    public class AdCampaign
    {
        public int Id { get; set; }

        public int PagePostId { get; set; }
        public virtual PagePost? PagePost { get; set; }

        [Required]
        //public string Status { get; set; } = "Draft"; // Draft, Active, Paused, Completed
        public CampaignStatus Status { get; set; } = CampaignStatus.Draft; // <-- ئەم هێڵە نوێیە دابنێ

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Budget { get; set; }
        public string Currency { get; set; } = "USD";

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // ئۆبجێکتی ئامانج لێرەدا دادەنرێت
        public TargetAudience? Audience { get; set; }
    }
    public class Transaction
    {
        public int Id { get; set; }
        public int AdCampaignId { get; set; } // کام کەمپەین پارەی بۆ دراوە
        public virtual AdCampaign? AdCampaign { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? PaymentGateway { get; set; } // ناوی دەروازەکە, e.g., "Switch", "ZainCash"
        public string? GatewayTransactionId { get; set; } // ناسنامەی مامەڵەکە لای دەروازەکە
        public string? Status { get; set; } // e.g., "Pending", "Completed", "Failed"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}

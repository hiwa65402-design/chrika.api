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
        public string Status { get; set; } = "Draft"; // Draft, Active, Paused, Completed

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Budget { get; set; }
        public string Currency { get; set; } = "USD";

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // ئۆبجێکتی ئامانج لێرەدا دادەنرێت
        public TargetAudience? Audience { get; set; }
    }
}

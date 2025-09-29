using Microsoft.EntityFrameworkCore;

namespace Chrika.Api.Models
{
    [Owned] // ئەمە وا دەکات وەک بەشێک لە خشتەی AdCampaign خەزن بکرێت
    public class TargetAudience
    {
        public List<string> Locations { get; set; } = new();
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public string? Gender { get; set; } // "Male", "Female", "All"
    }
}

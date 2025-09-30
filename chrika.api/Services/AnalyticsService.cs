using Chrika.Api.Data;
using Chrika.Api.Models;
using Microsoft.EntityFrameworkCore; // دڵنیابە ئەمە هەیە
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task RecordInteractionAsync(int adCampaignId, InteractionType type, int? userId)
        {
            // === زیادکرا: لێرەدا دەپشکنین ئایا کەمپینەکە بوونی هەیە ===
            var campaignExists = await _context.AdCampaigns.AnyAsync(c => c.Id == adCampaignId);

            // ئەگەر کەمپینەکە بوونی نەبوو، هیچ ناکەین و دەوەستین بۆ ئەوەی هەڵە ڕوونەدات
            if (!campaignExists)
            {
                return; // گرنگترین بەش ئەمەیە
            }

            // ئەگەر کەمپینەکە بوونی هەبوو، ئەوا کارلێکەکە زیاد دەکەین
            var interaction = new AdInteraction
            {
                AdCampaignId = adCampaignId,
                Type = type,
                InteractingUserId = userId
            };

            _context.AdInteractions.Add(interaction);
            await _context.SaveChangesAsync();
        }
    }
}

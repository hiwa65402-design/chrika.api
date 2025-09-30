using Chrika.Api.Data;
using Chrika.Api.DTOs; // بۆ CampaignAnalyticsDto
using Chrika.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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

        // فانکشنی تۆمارکردنی کارلێک (لایک، کۆمێنت، کلیک)
        public async Task RecordInteractionAsync(int adCampaignId, InteractionType type, int? userId)
        {
            var campaignExists = await _context.AdCampaigns.AnyAsync(c => c.Id == adCampaignId);
            if (!campaignExists) return;

            var interaction = new AdInteraction
            {
                AdCampaignId = adCampaignId,
                Type = type, // جۆرەکە لە دەرەوە دیاری دەکرێت
                InteractingUserId = userId
            };

            _context.AdInteractions.Add(interaction);
            await _context.SaveChangesAsync();
        }

        // فانکشنی تۆمارکردنی بینین
        public async Task RecordImpressionAsync(int adCampaignId, int? userId)
        {
            // بۆ خێرایی، ئەم فانکشنە دەتوانێت ڕاستەوخۆ بانگی فانکشنەکەی سەرەوە بکات
            await RecordInteractionAsync(adCampaignId, InteractionType.Impression, userId);
        }

        // فانکشنی وەرگرتنەوەی ئامارەکان
        public async Task<CampaignAnalyticsDto?> GetCampaignAnalyticsAsync(int campaignId)
        {
            var campaign = await _context.AdCampaigns
                .AsNoTracking() // بۆ خێراکردنی خوێندنەوە
                .Include(c => c.PagePost)
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign == null)
            {
                return null; // ئەگەر کەمپینەکە نەدۆزرایەوە
            }

            // هەموو کارلێکەکان بە یەکجار لە بنکەی دراوە وەردەگرین
            var interactions = await _context.AdInteractions
                .Where(i => i.AdCampaignId == campaignId)
                .AsNoTracking()
                .ToListAsync();

            // دروستکردنی ئۆبجێکتی ئەنجام
            var analytics = new CampaignAnalyticsDto
            {
                CampaignId = campaign.Id,
                PostContent = campaign.PagePost?.Content, // ناوەڕۆکی پۆستەکە
                TotalImpressions = interactions.Count(i => i.Type == InteractionType.Impression),
                TotalLikes = interactions.Count(i => i.Type == InteractionType.Like),
                TotalComments = interactions.Count(i => i.Type == InteractionType.Comment),
                TotalClicks = interactions.Count(i => i.Type == InteractionType.Click)
            };

            return analytics;
        }
    }
}

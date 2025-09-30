using Chrika.Api.DTOs; // بۆ CampaignAnalyticsDto
using Chrika.Api.Models;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface IAnalyticsService
    {
        // ئەمە بۆ تۆمارکردنی لایک، کۆمێنت، و کلیک بەکاردێت
        Task RecordInteractionAsync(int adCampaignId, InteractionType type, int? userId);

        // ئەمە بۆ تۆمارکردنی بینین (Impression) بەکاردێت
        Task RecordImpressionAsync(int adCampaignId, int? userId);

        // ئەمە بۆ وەرگرتنەوەی ئاماری کەمپینێک بەکاردێت
        Task<CampaignAnalyticsDto?> GetCampaignAnalyticsAsync(int campaignId);
    }
}

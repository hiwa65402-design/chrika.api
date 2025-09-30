using Chrika.Api.Data;
using Chrika.Api.Models;
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

using Chrika.Api.Models;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface IAnalyticsService
    {
        Task RecordInteractionAsync(int adCampaignId, InteractionType type, int? userId);
    }
}

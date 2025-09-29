using Chrika.Api.Models;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(int userId, int triggeredByUserId, NotificationType type, int? entityId);
    }
}

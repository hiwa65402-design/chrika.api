using Chrika.Api.Models;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(int userId, int triggeredByUserId, NotificationType type, int? entityId);
        string GenerateNotificationMessage(NotificationType type, string username);

    }
}

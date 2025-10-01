using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface ILikeService
    {
        // فانکشنێکی گشتی بۆ لایککردن
        Task<bool> ToggleLikeAsync(int entityId, string entityType, int userId);
    }
}

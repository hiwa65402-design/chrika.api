using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface IFollowService
    {
        // followerId: ئەو کەسەی کە فۆڵۆ دەکات (لە تۆکنەکەوە وەردەگیرێت)
        // followingId: ئەو کەسەی کە فۆڵۆ دەکرێت (لە URL ـەوە وەردەگیرێت)
        Task<bool> ToggleFollowAsync(int followerId, int followingId);
    }
}

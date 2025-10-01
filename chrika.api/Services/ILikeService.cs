// Services/ILikeService.cs

using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface ILikeService
    {
        /// <summary>
        /// لایکێک زیاد دەکات یان لای دەبات بۆ پۆستێکی ئاسایی یان پۆستێکی گرووپ.
        /// </summary>
        /// <param name="postId">IDی پۆستی ئاسایی (ئەگەر جۆرەکەی ئەوە بێت).</param>
        /// <param name="groupPostId">IDی پۆستی گرووپ (ئەگەر جۆرەکەی ئەوە بێت).</param>
        /// <param name="userId">IDی ئەو بەکارهێنەرەی کە لایکەکە دەکات.</param>
        /// <returns>True ئەگەر سەرکەوتوو بوو.</returns>
        Task<bool> ToggleLikeAsync(int? postId, int? groupPostId, int userId);
    }
}

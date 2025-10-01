// Services/IVideoService.cs

using Chrika.Api.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface IVideoService
    {
        // `pageNumber` و `pageSize` بۆ infinite scroll بەکاردێت
        Task<IEnumerable<VideoFeedItemDto>> GetVideoFeedAsync(int? userId, int pageNumber, int pageSize);
    }
}

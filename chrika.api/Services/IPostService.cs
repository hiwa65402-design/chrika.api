using Chrika.Api.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface IPostService
    {
        // فانکشنی  Feedـی گشتگیر
        Task<IEnumerable<FeedItemDto>> GetUniversalFeedAsync(int? userId);

        Task<IEnumerable<PostDto>> GetAllPostsAsync(int? currentUserId = null);
        Task<PostDto?> GetPostByIdAsync(int id, int? currentUserId = null);
        Task<PostDto> CreatePostAsync(CreatePostDto createPostDto, int userId);
        Task<PostDto?> UpdatePostAsync(int postId, UpdatePostDto updatePostDto, int userId);
        Task<bool> DeletePostAsync(int postId, int userId);
    }
}

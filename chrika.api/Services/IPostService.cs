using Chrika.Api.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface IPostService
    {
        // === گۆڕانکاری لێرەدایە ===
        Task<IEnumerable<PostDto>> GetAllPostsAsync(int? currentUserId = null);

        // === گۆڕانکاری لێرەدایە ===
        Task<PostDto?> GetPostByIdAsync(int id, int? currentUserId = null);

        Task<IEnumerable<PostDto>> GetFeedForUserAsync(int userId);
        Task<PostDto> CreatePostAsync(CreatePostDto createPostDto, int userId);
        Task<PostDto?> UpdatePostAsync(int postId, UpdatePostDto updatePostDto, int userId);
        Task<bool> DeletePostAsync(int postId, int userId);
    }
}

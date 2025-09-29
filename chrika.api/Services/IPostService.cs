using Chrika.Api.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface IPostService
    {
        Task<IEnumerable<PostDto>> GetAllPostsAsync();
        Task<PostDto?> GetPostByIdAsync(int id);
        // UserId ـمان پێویستە بۆ ئەوەی بزانین کێ پۆستەکە دروست دەکات
        Task<PostDto> CreatePostAsync(CreatePostDto createPostDto, int userId);
        Task<PostDto?> UpdatePostAsync(int postId, UpdatePostDto updatePostDto, int userId);
        Task<bool> DeletePostAsync(int postId, int userId);
        Task<IEnumerable<PostDto>> GetFeedForUserAsync(int userId);

    }
}

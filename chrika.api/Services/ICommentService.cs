using Chrika.Api.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface ICommentService
    {
        Task<CommentDto?> CreateCommentAsync(int postId, CreateCommentDto createCommentDto, int userId);
        Task<IEnumerable<CommentDto>> GetCommentsForPostAsync(int postId);
        Task<bool> DeleteCommentAsync(int commentId, int userId);
    }
}

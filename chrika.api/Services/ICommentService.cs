using Chrika.Api.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface ICommentService
    {
        Task<CommentDto?> CreateCommentAsync(int entityId, string entityType, CreateCommentDto dto, int userId);
        Task<IEnumerable<CommentDto>> GetCommentsAsync(int entityId, string entityType);
        Task<bool> DeleteCommentAsync(int commentId, int userId);
    }
}

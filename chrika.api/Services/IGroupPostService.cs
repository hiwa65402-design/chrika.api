using Chrika.Api.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface IGroupPostService
    {
        Task<GroupPostDto?> CreateGroupPostAsync(int groupId, CreateGroupPostDto createDto, int authorId);
        Task<IEnumerable<GroupPostDto>?> GetPostsForGroupAsync(int groupId, int currentUserId);
    }
}

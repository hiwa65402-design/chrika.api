using Chrika.Api.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace Chrika.Api.Services
{
    public interface IGroupService
    {
        Task<GroupDto> CreateGroupAsync(CreateGroupDto createGroupDto, int ownerId);
        Task<bool> UsernameExistsAsync(string username);


        Task<GroupDto?> GetGroupByIdAsync(int groupId);
        Task<IEnumerable<GroupDto>> GetAllPublicGroupsAsync();
        Task<bool> JoinGroupAsync(int groupId, int userId);
        Task<bool> LeaveGroupAsync(int groupId, int userId);
        Task<bool> FollowGroupAsync(int groupId, int userId);
        Task<bool> UnfollowGroupAsync(int groupId, int userId);
        Task<GroupDto?> UpdateGroupAsync(int groupId, UpdateGroupDto updateDto, int userId);

    }
}

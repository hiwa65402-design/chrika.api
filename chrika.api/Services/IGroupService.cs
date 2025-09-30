using Chrika.Api.DTOs;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface IGroupService
    {
        Task<GroupDto> CreateGroupAsync(CreateGroupDto createGroupDto, int ownerId);
        Task<bool> UsernameExistsAsync(string username);
    }
}

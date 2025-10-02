// Services/IGroupManagementService.cs

using Chrika.Api.DTOs;
using Chrika.Api.Models; // بۆ GroupRole
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public interface IGroupManagementService
    {
        /// <summary>
        /// گروپێکی نوێ دروست دەکات
        /// </summary>
        Task<GroupDto> CreateGroupAsync(CreateGroupDto dto, int creatorId);

        /// <summary>
        /// زانیارییەکانی گروپێک نوێ دەکاتەوە
        /// </summary>
        Task<bool> UpdateGroupInfoAsync(int groupId, UpdateGroupDto dto, int currentUserId);

        /// <summary>
        /// گروپێک بە تەواوی دەسڕێتەوە
        /// </summary>
        Task<bool> DeleteGroupAsync(int groupId, int currentUserId);

        /// <summary>
        /// ئەندامێکی نوێ بۆ گروپێک زیاد دەکات
        /// </summary>
        Task<bool> AddMemberAsync(int groupId, int userIdToAdd, int currentUserId);

        /// <summary>
        /// ئەندامێک لە گروپێک لادەبات
        /// </summary>
        Task<bool> RemoveMemberAsync(int groupId, int userIdToRemove, int currentUserId);

        /// <summary>
        /// بەکارهێنەر خۆی لە گروپێک دەکاتە دەرەوە
        /// </summary>
        Task<bool> LeaveGroupAsync(int groupId, int currentUserId);

        /// <summary>
        /// ڕۆڵی ئەندامێک دەگۆڕێت
        /// </summary>
        Task<bool> ChangeMemberRoleAsync(int groupId, int userIdToChange, GroupRole newRole, int currentUserId);

        /// <summary>
        /// زانیاری تەواوی گروپێک دەگەڕێنێتەوە
        /// </summary>
        Task<GroupDto> GetGroupDetailsAsync(int groupId);

        /// <summary>
        /// لیستی ئەندامانی گروپێک دەگەڕێنێتەوە
        /// </summary>
        Task<IEnumerable<GroupMemberDto>> GetGroupMembersAsync(int groupId);
    }
}

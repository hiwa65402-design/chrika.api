// Services/GroupManagementService.cs

using Chrika.Api.Data;
using Chrika.Api.DTOs;
using Chrika.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class GroupManagementService : IGroupManagementService
    {
        private readonly ApplicationDbContext _context;

        public GroupManagementService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ١. دروستکردنی گروپ
        public async Task<GroupDto> CreateGroupAsync(CreateGroupDto dto, int creatorId)
        {
            if (await _context.Groups.AnyAsync(g => g.Username == dto.Username))
            {
                return null;
            }

            var group = new Group
            {
                Name = dto.Name,
                Username = dto.Username,
                Description = dto.Description,
                Type = dto.Type,
                OwnerId = creatorId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            var ownerMember = new GroupMember
            {
                GroupId = group.Id,
                UserId = creatorId,
                Role = GroupRole.Owner
            };
            _context.GroupMembers.Add(ownerMember);
            await _context.SaveChangesAsync();

            return await GetGroupDetailsAsync(group.Id);
        }

        // ٢. نوێکردنەوەی زانیاری گروپ
        public async Task<bool> UpdateGroupInfoAsync(int groupId, UpdateGroupDto dto, int currentUserId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null || group.OwnerId != currentUserId)
            {
                return false; // یان خاوەنی گروپ نییە یان گروپ بوونی نییە
            }

            group.Name = dto.Name ?? group.Name;
            group.Description = dto.Description ?? group.Description;
            group.Bio = dto.Bio ?? group.Bio;
            group.ProfilePictureUrl = dto.ProfilePictureUrl ?? group.ProfilePictureUrl;
            group.CoverPictureUrl = dto.CoverPictureUrl ?? group.CoverPictureUrl;

            _context.Groups.Update(group);
            return await _context.SaveChangesAsync() > 0;
        }

        // ٣. سڕینەوەی گروپ
        public async Task<bool> DeleteGroupAsync(int groupId, int currentUserId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null || group.OwnerId != currentUserId)
            {
                return false; // تەنها خاوەنی گروپ دەتوانێت بیسڕێتەوە
            }

            _context.Groups.Remove(group);
            return await _context.SaveChangesAsync() > 0;
        }

        // ٤. زیادکردنی ئەندام
        public async Task<bool> AddMemberAsync(int groupId, int userIdToAdd, int currentUserId)
        {
            var group = await _context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null) return false;

            var currentUserRole = group.Members.FirstOrDefault(m => m.UserId == currentUserId)?.Role;
            if (currentUserRole != GroupRole.Owner && currentUserRole != GroupRole.Admin)
            {
                return false; // تەنها خاوەن و ئەدمین دەتوانن ئەندام زیاد بکەن
            }

            if (group.Members.Any(m => m.UserId == userIdToAdd))
            {
                return true; // پێشتر ئەندامە
            }

            var newMember = new GroupMember { GroupId = groupId, UserId = userIdToAdd, Role = GroupRole.Member };
            _context.GroupMembers.Add(newMember);
            return await _context.SaveChangesAsync() > 0;
        }

        // ٥. لابردنی ئەندام
        public async Task<bool> RemoveMemberAsync(int groupId, int userIdToRemove, int currentUserId)
        {
            var group = await _context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null) return false;

            var currentUserRole = group.Members.FirstOrDefault(m => m.UserId == currentUserId)?.Role;
            var memberToRemove = group.Members.FirstOrDefault(m => m.UserId == userIdToRemove);

            if (memberToRemove == null) return true; // ئەندامەکە بوونی نییە
            if (memberToRemove.Role == GroupRole.Owner) return false; // ناتوانرێت خاوەنی گروپ لاببرێت

            if (currentUserRole == GroupRole.Owner || (currentUserRole == GroupRole.Admin && memberToRemove.Role == GroupRole.Member))
            {
                _context.GroupMembers.Remove(memberToRemove);
                return await _context.SaveChangesAsync() > 0;
            }

            return false;
        }

        // ٦. خۆ دەرکردن لە گروپ
        public async Task<bool> LeaveGroupAsync(int groupId, int currentUserId)
        {
            var member = await _context.GroupMembers.FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId);
            if (member == null || member.Role == GroupRole.Owner)
            {
                // یان ئەندام نییە یان خاوەنی گروپە و ناتوانێت جێیبهێڵێت
                return false;
            }

            _context.GroupMembers.Remove(member);
            return await _context.SaveChangesAsync() > 0;
        }

        // ٧. گۆڕینی ڕۆڵی ئەندام
        public async Task<bool> ChangeMemberRoleAsync(int groupId, int userIdToChange, GroupRole newRole, int currentUserId)
        {
            var group = await _context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null || group.OwnerId != currentUserId)
            {
                return false; // تەنها خاوەنی گروپ دەتوانێت ڕۆڵ بگۆڕێت
            }

            var memberToChange = group.Members.FirstOrDefault(m => m.UserId == userIdToChange);
            if (memberToChange == null || memberToChange.Role == GroupRole.Owner)
            {
                return false; // ئەندام بوونی نییە یان خاوەنی گروپە
            }

            memberToChange.Role = newRole;
            _context.GroupMembers.Update(memberToChange);
            return await _context.SaveChangesAsync() > 0;
        }

        // ٨. هێنانی زانیاری تەواوی گروپ
        public async Task<GroupDto> GetGroupDetailsAsync(int groupId)
        {
            var group = await _context.Groups
                .Include(g => g.Owner)
                .Include(g => g.Members)
                .Include(g => g.Followers)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            return MapToGroupDto(group);
        }

        // ٩. هێنانی لیستی ئەندامانی گروپ
        public async Task<IEnumerable<GroupMemberDto>> GetGroupMembersAsync(int groupId)
        {
            return await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId)
                .Include(gm => gm.User)
                .Select(gm => new GroupMemberDto
                {
                    UserId = gm.UserId,
                    Username = gm.User.Username,
                    ProfilePictureUrl = gm.User.ProfilePicture,
                    Role = gm.Role.ToString(),
                    JoinedAt = gm.JoinedAt
                })
                .ToListAsync();
        }

        // فانکشنێکی یاریدەدەر
        private GroupDto MapToGroupDto(Group group)
        {
            if (group == null) return null;
            return new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Username = group.Username,
                Description = group.Description,
                Bio = group.Bio,
                ProfilePictureUrl = group.ProfilePictureUrl,
                CoverPictureUrl = group.CoverPictureUrl,
                Type = group.Type.ToString(),
                OwnerUsername = group.Owner?.Username,
                MemberCount = group.Members?.Count ?? 0,
                FollowerCount = group.Followers?.Count ?? 0,
                CreatedAt = group.CreatedAt
            };
        }
    }
}

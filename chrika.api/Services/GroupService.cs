using Chrika.Api.Data;
using Chrika.Api.DTOs;
using Chrika.Api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Chrika.Api.Services
{
    public class GroupService : IGroupService
    {
        private readonly ApplicationDbContext _context;

        public GroupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Groups.AnyAsync(g => g.Username.ToLower() == username.ToLower());
        }

        public async Task<GroupDto> CreateGroupAsync(CreateGroupDto createGroupDto, int ownerId)
        {
            // 1. دروستکردنی ئۆبجێکتی گرووپ
            var group = new Group
            {
                Name = createGroupDto.Name,
                Username = createGroupDto.Username,
                Description = createGroupDto.Description,
                Type = createGroupDto.Type,
                OwnerId = ownerId
            };

            // ئەگەر گرووپەکە کاتی بوو، کاتی سڕینەوەی بۆ دابنێ
            if (group.Type == GroupType.Temporary)
            {
                group.ExpiresAt = DateTime.UtcNow.AddHours(24);
            }

            // 2. زیادکردنی گرووپ بۆ بنکەی دراوە
            _context.Groups.Add(group);
            await _context.SaveChangesAsync(); // لێرەدا پاشەکەوتی دەکەین بۆ ئەوەی group.Id وەربگرین

            // 3. زیادکردنی خاوەنی گرووپ وەک یەکەم ئەندام و ڕۆڵی Owner
            var ownerMember = new GroupMember
            {
                GroupId = group.Id,
                UserId = ownerId,
                Role = GroupRole.Owner,
                JoinedAt = DateTime.UtcNow
            };
            _context.GroupMembers.Add(ownerMember);
            await _context.SaveChangesAsync();

            // 4. گەڕاندنەوەی زانیاری گرووپە نوێیەکە وەک DTO
            var owner = await _context.Users.FindAsync(ownerId);
            return new GroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Username = group.Username,
                Description = group.Description,
                Type = group.Type.ToString(),
                OwnerUsername = owner.Username,
                MemberCount = 1, // یەکەم ئەندام خاوەنەکەیەتی
                FollowerCount = 0,
                CreatedAt = group.CreatedAt
            };
        }
    
            public async Task<GroupDto?> GetGroupByIdAsync(int groupId)
        {
            var group = await _context.Groups
                .AsNoTracking()
                .Include(g => g.Owner) // بۆ وەرگرتنی ناوی خاوەنەکەی
                .Include(g => g.Members) // بۆ ژماردنی ئەندامەکان
                .Include(g => g.Followers) // بۆ ژماردنی فۆڵۆوەرەکان
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                return null; // ئەگەر گرووپەکە نەدۆزرایەوە
            }

            // گۆڕینی بۆ DTO
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
                OwnerUsername = group.Owner.Username,
                MemberCount = group.Members.Count,
                FollowerCount = group.Followers.Count,
                CreatedAt = group.CreatedAt
            };
        }

        // فانکشنی وەرگرتنی هەموو گرووپە گشتییەکان
        public async Task<IEnumerable<GroupDto>> GetAllPublicGroupsAsync()
        {
            return await _context.Groups
                .AsNoTracking()
                .Where(g => g.Type == GroupType.Public) // تەنها گرووپە گشتییەکان
                .Include(g => g.Owner)
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new GroupDto // ڕاستەوخۆ لێرەدا دەیگۆڕین بۆ DTO
                {
                    Id = g.Id,
                    Name = g.Name,
                    Username = g.Username,
                    Description = g.Description,
                    ProfilePictureUrl = g.ProfilePictureUrl,
                    Type = g.Type.ToString(),
                    OwnerUsername = g.Owner.Username,
                    MemberCount = g.Members.Count(), // ژماردنی ئەندامەکان
                    FollowerCount = g.Followers.Count(), // ژماردنی فۆڵۆوەرەکان
                    CreatedAt = g.CreatedAt
                })
                .ToListAsync();
        }
  

        public async Task<bool> JoinGroupAsync(int groupId, int userId)
        {
            // 1. پشکنینی بوونی گرووپ
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null || group.Type != GroupType.Public)
            {
                // یان گرووپەکە بوونی نییە، یان گشتی نییە
                return false;
            }

            // 2. پشکنینی ئەوەی کە ئایا بەکارهێنەرەکە پێشتر ئەندامە
            var isAlreadyMember = await _context.GroupMembers
                .AnyAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (isAlreadyMember)
            {
                // پێشتر ئەندامە، پێویست بە هیچ ناکات
                return true;
            }

            // 3. زیادکردنی بەکارهێنەر وەک ئەندامی نوێ
            var newMember = new GroupMember
            {
                GroupId = groupId,
                UserId = userId,
                Role = GroupRole.Member // ڕۆڵی ئەندامی ئاسایی
            };

            _context.GroupMembers.Add(newMember);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> LeaveGroupAsync(int groupId, int userId)
        {
            // 1. دۆزینەوەی تۆماری ئەندامێتی
            var membership = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (membership == null)
            {
                // ئەگەر بەکارهێنەرەکە ئەندام نەبێت، هیچ ناکەین
                return false;
            }

            // 2. پشکنینی ئەوەی کە ئایا خاوەنی گرووپە
            if (membership.Role == GroupRole.Owner)
            {
                // خاوەنی گرووپ ناتوانێت گرووپەکەی جێبهێڵێت، دەبێت بیسڕێتەوە
                // ئەمە بۆ دواتر چارەسەر دەکەین
                return false;
            }

            // 3. سڕینەوەی تۆماری ئەندامێتی
            _context.GroupMembers.Remove(membership);
            await _context.SaveChangesAsync();

            return true;
        }

      
        public async Task<bool> FollowGroupAsync(int groupId, int userId)
        {
            // 1. پشکنینی بوونی گرووپ
            var groupExists = await _context.Groups.AnyAsync(g => g.Id == groupId && g.Type == GroupType.Public);
            if (!groupExists)
            {
                return false; // گرووپەکە بوونی نییە یان گشتی نییە
            }

            // 2. پشکنینی ئەوەی کە ئایا بەکارهێنەرەکە پێشتر فۆڵۆی کردووە
            var isAlreadyFollowing = await _context.GroupFollowers
                .AnyAsync(f => f.GroupId == groupId && f.UserId == userId);

            if (isAlreadyFollowing)
            {
                return true; // پێشتر فۆڵۆی کردووە
            }

            // 3. زیادکردنی فۆڵۆوەری نوێ
            var newFollower = new GroupFollower
            {
                GroupId = groupId,
                UserId = userId
            };

            _context.GroupFollowers.Add(newFollower);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnfollowGroupAsync(int groupId, int userId)
        {
            // 1. دۆزینەوەی تۆماری فۆڵۆکردن
            var follow = await _context.GroupFollowers
                .FirstOrDefaultAsync(f => f.GroupId == groupId && f.UserId == userId);

            if (follow == null)
            {
                return false; // ئەگەر بەکارهێنەرەکە فۆڵۆی نەکردبێت
            }

            // 2. سڕینەوەی تۆماری فۆڵۆکردن
            _context.GroupFollowers.Remove(follow);
            await _context.SaveChangesAsync();

            return true;
        }


    }
}

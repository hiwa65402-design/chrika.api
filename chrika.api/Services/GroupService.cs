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

   

        public async Task<GroupDto?> UpdateGroupAsync(int groupId, UpdateGroupDto updateDto, int userId)
        {
            // 1. دۆزینەوەی گرووپ
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                return null; // گرووپ بوونی نییە
            }

            // 2. === پشکنینی دەسەڵات ===
            // ئایا بەکارهێنەرەکە ئەندامە و ڕۆڵی ئۆنەر یان ئەدمینە؟
            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (member == null || (member.Role != GroupRole.Owner && member.Role != GroupRole.Admin))
            {
                // ئەگەر بەکارهێنەرەکە ئەندام نەبوو، یان ڕۆڵەکەی Owner/Admin نەبوو، ڕێگەی پێنادرێت
                return null;
            }

            // 3. نوێکردنەوەی زانیارییەکان
            // تەنها ئەو زانیاریانە نوێ دەکەینەوە کە لە DTOـکەدا نێردراون
            if (!string.IsNullOrEmpty(updateDto.Name))
            {
                group.Name = updateDto.Name;
            }
            if (updateDto.Description != null)
            {
                group.Description = updateDto.Description;
            }
            if (updateDto.Bio != null)
            {
                group.Bio = updateDto.Bio;
            }
            if (updateDto.ProfilePictureUrl != null)
            {
                group.ProfilePictureUrl = updateDto.ProfilePictureUrl;
            }
            if (updateDto.CoverPictureUrl != null)
            {
                group.CoverPictureUrl = updateDto.CoverPictureUrl;
            }

            // 4. پاشەکەوتکردنی گۆڕانکارییەکان
            await _context.SaveChangesAsync();

            // 5. گەڕاندنەوەی زانیاری نوێکراوەی گرووپەکە
            return await GetGroupByIdAsync(groupId);
        }


        public async Task<bool> PromoteMemberAsync(int groupId, int targetUserId, int currentUserId)
        {
            // 1. پشکنینی دەسەڵاتی بەکارهێنەری ئێستا (دەبێت ئۆنەر بێت)
            var currentUserMembership = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId);

            if (currentUserMembership?.Role != GroupRole.Owner)
            {
                // تەنها خاوەنی گرووپ دەتوانێت کەسێک بکات بە ئەدمین
                return false;
            }

            // 2. دۆزینەوەی ئەندامی ئامانج
            var targetMember = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == targetUserId);

            // 3. دڵنیابوونەوە لەوەی کە ئەندامەکە بوونی هەیە و ڕۆڵەکەی Memberـە
            if (targetMember == null || targetMember.Role != GroupRole.Member)
            {
                // یان ئەندامەکە بوونی نییە، یان پێشتر ئەدمین/ئۆنەرە
                return false;
            }

            // 4. گۆڕینی ڕۆڵ
            targetMember.Role = GroupRole.Admin;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DemoteMemberAsync(int groupId, int targetUserId, int currentUserId)
        {
            // 1. پشکنینی دەسەڵاتی بەکارهێنەری ئێستا (دەبێت ئۆنەر بێت)
            var currentUserMembership = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId);

            if (currentUserMembership?.Role != GroupRole.Owner)
            {
                return false;
            }

            // 2. دۆزینەوەی ئەندامی ئامانج
            var targetMember = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == targetUserId);

            // 3. دڵنیابوونەوە لەوەی کە ئەندامەکە بوونی هەیە و ڕۆڵەکەی Adminـە
            if (targetMember == null || targetMember.Role != GroupRole.Admin)
            {
                return false;
            }

            // 4. گۆڕینی ڕۆڵ
            targetMember.Role = GroupRole.Member;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> KickMemberAsync(int groupId, int targetUserId, int currentUserId)
        {
            // 1. پشکنینی دەسەڵاتی بەکارهێنەری ئێستا (ئۆنەر یان ئەدمین)
            var currentUserMembership = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId);

            if (currentUserMembership == null || (currentUserMembership.Role != GroupRole.Owner && currentUserMembership.Role != GroupRole.Admin))
            {
                return false;
            }

            // 2. دۆزینەوەی ئەندامی ئامانج
            var targetMember = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == targetUserId);

            if (targetMember == null)
            {
                return false; // ئەندامەکە بوونی نییە
            }

            // 3. === یاسای گرنگ: کێ دەتوانێت کێ دەربکات؟ ===
            // ئۆنەر ناتوانرێت دەربکرێت
            if (targetMember.Role == GroupRole.Owner) return false;
            // ئەدمین ناتوانێت ئەدمینێکی تر دەربکات (تەنها ئۆنەر دەتوانێت)
            if (targetMember.Role == GroupRole.Admin && currentUserMembership.Role != GroupRole.Owner) return false;

            // 4. سڕینەوەی ئەندامێتی
            _context.GroupMembers.Remove(targetMember);
            await _context.SaveChangesAsync();

            return true;
        }

       
        public async Task<bool> RequestToJoinGroupAsync(int groupId, int userId)
        {
            // 1. پشکنینی بوونی گرووپ و جۆرەکەی
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null || group.Type != GroupType.Private)
            {
                return false; // گرووپەکە بوونی نییە یان Private نییە
            }

            // 2. پشکنینی ئەوەی کە ئایا بەکارهێنەرەکە پێشتر ئەندامە یان داواکاری ناردووە
            var isMember = await _context.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId);
            var hasPendingRequest = await _context.GroupJoinRequests.AnyAsync(r => r.GroupId == groupId && r.UserId == userId && r.Status == RequestStatus.Pending);

            if (isMember || hasPendingRequest)
            {
                return false; // پێشتر ئەندامە یان داواکارییەکی چاوەڕوانکراوی هەیە
            }

            // 3. دروستکردنی داواکاری نوێ
            var request = new GroupJoinRequest
            {
                GroupId = groupId,
                UserId = userId
            };

            _context.GroupJoinRequests.Add(request);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<GroupJoinRequestDto>> GetJoinRequestsAsync(int groupId, int currentUserId)
        {
            // 1. پشکنینی دەسەڵاتی بەکارهێنەری ئێستا (دەبێت ئۆنەر یان ئەدمین بێت)
            var isAuthorized = await _context.GroupMembers
                .AnyAsync(m => m.GroupId == groupId && m.UserId == currentUserId && (m.Role == GroupRole.Owner || m.Role == GroupRole.Admin));

            if (!isAuthorized)
            {
                return null; // یان لیستی بەتاڵ دەگەڕێنینەوە
            }

            // 2. هێنانی هەموو داواکارییە چاوەڕوانکراوەکان
            return await _context.GroupJoinRequests
                .Where(r => r.GroupId == groupId && r.Status == RequestStatus.Pending)
                .Include(r => r.User) // بۆ وەرگرتنی زانیاری بەکارهێنەر
                .Select(r => new GroupJoinRequestDto
                {
                    RequestId = r.Id,
                    UserId = r.UserId,
                    Username = r.User.Username,
                    UserProfilePicture = r.User.ProfilePicture,
                    RequestedAt = r.RequestedAt
                })
                .ToListAsync();
        }

        public async Task<bool> ProcessJoinRequestAsync(int requestId, bool accept, int currentUserId)
        {
            // 1. دۆزینەوەی داواکارییەکە
            var request = await _context.GroupJoinRequests.Include(r => r.Group).FirstOrDefaultAsync(r => r.Id == requestId);
            if (request == null || request.Status != RequestStatus.Pending)
            {
                return false; // داواکارییەکە بوونی نییە یان پێشتر پرۆسێس کراوە
            }

            // 2. پشکنینی دەسەڵاتی بەکارهێنەری ئێستا
            var isAuthorized = await _context.GroupMembers
                .AnyAsync(m => m.GroupId == request.GroupId && m.UserId == currentUserId && (m.Role == GroupRole.Owner || m.Role == GroupRole.Admin));

            if (!isAuthorized)
            {
                return false;
            }

            if (accept)
            {
                // ئەگەر قبوڵکرا
                request.Status = RequestStatus.Accepted;

                // زیادکردنی بەکارهێنەر وەک ئەندام
                var newMember = new GroupMember
                {
                    GroupId = request.GroupId,
                    UserId = request.UserId,
                    Role = GroupRole.Member
                };
                _context.GroupMembers.Add(newMember);
            }
            else
            {
                // ئەگەر ڕەتکرایەوە
                request.Status = RequestStatus.Rejected;
            }

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<IEnumerable<GroupMemberDto>?> GetGroupMembersAsync(int groupId, int? currentUserId)
        {
            // 1. دۆزینەوەی گرووپ
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                return new List<GroupMemberDto>();
            }

            // 2. پشکنینی دەسەڵات بۆ گرووپی Private
            if (group.Type == GroupType.Private)
            {
                // === گۆڕانکارییەکە لێرەدایە ===
                // ئەگەر بەکارهێنەر لۆگینی نەکردبێت (currentUserId is null)
                // یان ئەگەر لۆگینی کردبێت بەڵام ئەندام نەبێت
                if (currentUserId == null || !await _context.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == currentUserId.Value))
                {
                    return null; // ڕێگەی پێنادرێت
                }
            }

            // 3. هێنانی لیستی ئەندامان
            return await _context.GroupMembers
                .Where(m => m.GroupId == groupId)
                .Include(m => m.User)
                .OrderBy(m => m.Role)
                .ThenBy(m => m.JoinedAt)
                .Select(m => new GroupMemberDto
                {
                    UserId = m.UserId,
                    Username = m.User.Username,
                    ProfilePictureUrl = m.User.ProfilePicture,
                    Role = m.Role.ToString(),
                    JoinedAt = m.JoinedAt
                })
                .ToListAsync();
        }


    }
}

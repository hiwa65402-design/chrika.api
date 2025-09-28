using Chrika.Api.Data; // <-- ئەمە زیاد بکە بۆ کارکردن لەگەڵ داتابەیس
using Chrika.Api.DTOs;
using Chrika.Api.Models;
using Microsoft.EntityFrameworkCore; // <-- ئەمەش زیاد بکە

namespace Chrika.Api.Services
{
    public class UserService : IUserService
    {
        // --- بەشی یەکەم: گۆڕینی لیستی کاتی بە داتابەیسی ڕاستەقینە ---
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- بەشی دووەم: نوێکردنەوەی میتۆدەکان بۆ کارکردن لەگەڵ داتابەیس ---

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            // گۆڕینی هاشکردن بۆ BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

            var user = new User
            {
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                PasswordHash = passwordHash, // بەکارهێنانی هاشە نوێیەکە
                Bio = createUserDto.Bio,
                DateOfBirth = createUserDto.DateOfBirth,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                IsVerified = false
            };

            // زیادکردن بۆ داتابەیسی ڕاستەقینە
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return MapToDto(user);
        }

        public async Task<UserDto?> AuthenticateUserAsync(LoginDto loginDto)
        {
            // گەڕان لەناو داتابەیسی ڕاستەقینە
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                (u.Username.Equals(loginDto.UsernameOrEmail, StringComparison.OrdinalIgnoreCase) ||
                 u.Email.Equals(loginDto.UsernameOrEmail, StringComparison.OrdinalIgnoreCase)) &&
                u.IsActive);

            // بەکارهێنانی BCrypt بۆ بەراوردکردنی پاسۆرد
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null; // ئەگەر بەکارهێنەر نەبوو یان پاسۆرد هەڵە بوو
            }

            return MapToDto(user);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            // گەڕان لەناو داتابەیسی ڕاستەقینە
            return await _context.Users.AnyAsync(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.IsActive);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            // گەڕان لەناو داتابەیسی ڕاستەقینە
            return await _context.Users.AnyAsync(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.IsActive);
        }

        // ... میتۆدەکانی تریش دەبێت بە هەمان شێوە کار لەگەڵ _context بکەن ...
        // من بۆ کورتکردنەوە تەنها گرنگەکانم داناوە، ئەوانی تریش بە ئاسانی دەگۆڕدرێن

        #region (میتۆدەکانی تر - وەک خۆیان با بمێننەوە بەڵام دەبێت بگۆڕدرێن بۆ _context)

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users.Where(u => u.IsActive).ToListAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.IsActive);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.IsActive);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
            if (user == null) return null;

            // ... (لۆجیکی نوێکردنەوە وەک خۆی)
            user.FirstName = updateUserDto.FirstName ?? user.FirstName;
            user.LastName = updateUserDto.LastName ?? user.LastName;
            // ...
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return MapToDto(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Followings)
                .Include(u => u.Posts)
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

            if (user == null) return null;

            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                // ...
                FollowersCount = user.Followers.Count,
                FollowingCount = user.Followings.Count,
                PostsCount = user.Posts.Count
            };
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id && u.IsActive);
        }

        #endregion

        // ئەم میتۆدە وەک خۆی دەمێنێتەوە و زۆر باشە
        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePicture = user.ProfilePicture,
                Bio = user.Bio,
                DateOfBirth = user.DateOfBirth,
                CreatedAt = user.CreatedAt,
                IsVerified = user.IsVerified
            };
        }
    }
}

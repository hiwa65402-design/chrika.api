using Chrika.Api.Data;
using Chrika.Api.Dtos;
using Chrika.Api.DTOs;
using Chrika.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Chrika.Api.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService; // زیادکرا بۆ دروستکردنی تۆکن

        // Constructor نوێکرایەوە بۆ وەرگرتنی ITokenService
        public UserService(ApplicationDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

      
        public async Task<LoginResponseDto?> AuthenticateUserAsync(LoginDto loginDto)
        {
            // === گۆڕانکاری بۆ شێوازێکی ستانداردتر ===
            var normalizedInput = loginDto.UsernameOrEmail.ToLower();

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                (u.Username.ToLower() == normalizedInput || u.Email.ToLower() == normalizedInput) &&
                u.IsActive);
            // =======================================

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null; // Authentication failed
            }

            var token = _tokenService.CreateToken(user);

            return new LoginResponseDto
            {
                Username = user.Username,
                Token = token
            };
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

            var user = new User
            {
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                PasswordHash = passwordHash,
                Bio = createUserDto.Bio,
                DateOfBirth = createUserDto.DateOfBirth,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                IsVerified = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return MapToDto(user);
        }

        // ... باقی میتۆدەکان وەک خۆیان بمێننەوە (کۆدی پێشووت بێکێشە بوو) ...
        #region Other Methods
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

            user.FirstName = updateUserDto.FirstName ?? user.FirstName;
            user.LastName = updateUserDto.LastName ?? user.LastName;
            user.Bio = updateUserDto.Bio ?? user.Bio;
            user.ProfilePicture = updateUserDto.ProfilePicture ?? user.ProfilePicture;
            if (updateUserDto.DateOfBirth.HasValue) user.DateOfBirth = updateUserDto.DateOfBirth.Value;

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
                ProfilePicture = user.ProfilePicture,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt,
                IsVerified = user.IsVerified,
                FollowersCount = user.Followers.Count,
                FollowingCount = user.Followings.Count,
                PostsCount = user.Posts.Count
            };
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id && u.IsActive);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.IsActive);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.IsActive);
        }

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
        #endregion

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            // 1. بەکارهێنەرەکە لە داتابەیس بدۆزەرەوە
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false; // بەکارهێنەر بوونی نییە
            }

            // 2. دڵنیابە کە پاسۆردە کۆنەکە ڕاستە
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                return false; // پاسۆردی کۆن هەڵەیە
            }

            // 3. پاسۆردە نوێیەکە هاش بکە
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

            // 4. پاسۆردە نوێیەکە خەزن بکە
            user.PasswordHash = newPasswordHash;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> UpdateProfilePictureAsync(int userId, string imageUrl)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.ProfilePicture = imageUrl;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

    }

}

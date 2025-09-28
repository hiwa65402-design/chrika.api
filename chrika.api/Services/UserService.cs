using Chrika.Api.DTOs;
using Chrika.Api.Models;
using System.Security.Cryptography;
using System.Text;

namespace Chrika.Api.Services
{
    public class UserService : IUserService
    {
        private static readonly List<User> _users = new();
        private static int _nextId = 1;

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            await Task.Delay(1); // Simulate async operation
            return _users.Where(u => u.IsActive).Select(MapToDto);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            await Task.Delay(1); // Simulate async operation
            var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            await Task.Delay(1); // Simulate async operation
            var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.IsActive);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            await Task.Delay(1); // Simulate async operation
            var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.IsActive);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            await Task.Delay(1); // Simulate async operation

            var user = new User
            {
                Id = _nextId++,
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                PasswordHash = HashPassword(createUserDto.Password),
                Bio = createUserDto.Bio,
                DateOfBirth = createUserDto.DateOfBirth,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                IsVerified = false
            };

            _users.Add(user);
            return MapToDto(user);
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            await Task.Delay(1); // Simulate async operation
            var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
            
            if (user == null)
                return null;

            if (!string.IsNullOrEmpty(updateUserDto.FirstName))
                user.FirstName = updateUserDto.FirstName;
            
            if (!string.IsNullOrEmpty(updateUserDto.LastName))
                user.LastName = updateUserDto.LastName;
            
            if (updateUserDto.Bio != null)
                user.Bio = updateUserDto.Bio;
            
            if (!string.IsNullOrEmpty(updateUserDto.ProfilePicture))
                user.ProfilePicture = updateUserDto.ProfilePicture;
            
            if (updateUserDto.DateOfBirth.HasValue)
                user.DateOfBirth = updateUserDto.DateOfBirth.Value;

            user.UpdatedAt = DateTime.UtcNow;

            return MapToDto(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            await Task.Delay(1); // Simulate async operation
            var user = _users.FirstOrDefault(u => u.Id == id);
            
            if (user == null)
                return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            return true;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int id)
        {
            await Task.Delay(1); // Simulate async operation
            var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
            
            if (user == null)
                return null;

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
                FollowersCount = 0, // TODO: Implement when Follow functionality is added
                FollowingCount = 0, // TODO: Implement when Follow functionality is added
                PostsCount = 0      // TODO: Implement when Post functionality is added
            };
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            await Task.Delay(1); // Simulate async operation
            return _users.Any(u => u.Id == id && u.IsActive);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            await Task.Delay(1); // Simulate async operation
            return _users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.IsActive);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            await Task.Delay(1); // Simulate async operation
            return _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && u.IsActive);
        }

        public async Task<UserDto?> AuthenticateUserAsync(LoginDto loginDto)
        {
            await Task.Delay(1); // Simulate async operation
            var user = _users.FirstOrDefault(u => 
                (u.Username.Equals(loginDto.UsernameOrEmail, StringComparison.OrdinalIgnoreCase) ||
                 u.Email.Equals(loginDto.UsernameOrEmail, StringComparison.OrdinalIgnoreCase)) &&
                u.IsActive);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                return null;

            return MapToDto(user);
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

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }
}


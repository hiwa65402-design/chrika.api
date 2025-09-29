using Chrika.Api.Dtos;
using Chrika.Api.DTOs;

namespace Chrika.Api.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> GetUserByUsernameAsync(string username);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);
        Task<UserProfileDto?> GetUserProfileAsync(int id);
        Task<bool> UserExistsAsync(int id);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);

        // === گۆڕانکاری لێرەدایە ===
        // ئێستا لەبری UserDto، LoginResponseDto دەگەڕێنێتەوە کە تۆکنەکەی تێدایە
        Task<LoginResponseDto?> AuthenticateUserAsync(LoginDto loginDto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);

    }
}

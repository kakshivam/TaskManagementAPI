using TaskManagementAPI.DTOs;

namespace TaskManagementAPI.Services
{
    public interface IAuthService
    {
        Task<UserResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
        Task<UserResponseDto> RegisterWithRoleAsync(RegisterDto registerDto, string roleName);
    }
}

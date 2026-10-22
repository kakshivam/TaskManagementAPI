using TaskManagementAPI.DTOs;

namespace TaskManagementAPI.Services
{
    public interface IAuthService
    {
        Task<UserResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto, string ipAddress);
        Task<UserResponseDto> RegisterWithRoleAsync(RegisterDto registerDto, string roleName);

        Task<RefreshTokenResponseDto> RefreshTokenAsync(string token, string ipAddress);     // NEW
        Task<bool> RevokeTokenAsync(string token, string ipAddress);   //NEW

    }
}

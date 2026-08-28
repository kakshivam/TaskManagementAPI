using TaskManagementAPI.DTOs;

namespace TaskManagementAPI.Services
{
    public interface IAuthService
    {
        Task<UserResponseDto> RegisterAsync(RegisterDto registerDto);
    }
}

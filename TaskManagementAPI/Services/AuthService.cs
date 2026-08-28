using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using TaskManagementAPI.Data;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Model;

namespace TaskManagementAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<UserResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Check if the user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email ||
                                u.Username == registerDto.Username);

            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email and username already exists");
            }

            // Hash the password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Create new User
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            // Add to Database
            _context.Users.Add(user);  // in memory insertion
            await _context.SaveChangesAsync(); // persist to database

            //Return response DTO(no password)
            return new UserResponseDto
            {
                Id= user.Id,
                UserName = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagementAPI.Data;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Model;

namespace TaskManagementAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configration;

        public AuthService(ApplicationDbContext context, IConfiguration configration)
        {
            _context = context;
            _configration = configration;
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

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Find the user exist
            var user = await _context.Users
                 .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            // Check if the user exists
            if(user == null)
            {
                throw new UnauthorizedAccessException("Invalid Email or password");
            }

            // Verify Password
            bool isPassword = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

            if(!isPassword)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Generate JWT Toke
            var token = GenerateJwtToken(user);
            var expireAt = DateTime.UtcNow.AddMinutes(double.Parse(_configration["JwtSettings:ExpiryMinutes"]!));

            // Return login response
            return new LoginResponseDto
            {
                Token = token,
                User = new UserResponseDto
                {
                    Id = user.Id,
                    UserName= user.Username,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                },
                ExpiresAt = expireAt
            };
        }

        private string GenerateJwtToken(User user)
        {
            //Get JWT setting from configuration
            var secretKey = _configration["JwtSettings:SecretKey"];
            var issuer = _configration["JwtSettings:Issuer"];
            var audience = _configration["JwtSettings:Audience"];
            var expiryMinutes = double.Parse(_configration["JwtSettings:ExpiryMinutes"]!);

            // Create Secret Key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define claims (user information to include in token)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) 
            };

            // Create Token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
                );

            // Return token as string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

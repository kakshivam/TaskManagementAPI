using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskManagementAPI.Constants;
using TaskManagementAPI.Data;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Model;
using TaskManagementAPI.Repositories;

namespace TaskManagementAPI.Services
{
    public class AuthService : IAuthService
    {
        // Repositories replace direct DbContext usage
        // UserRepository    → all user-related DB operations
        // RefreshTokenRepository → all token-related DB operations
        // _context kept only for role lookup (no RoleRepository in this implementation)
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configration;

        public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository,
            ApplicationDbContext context, IConfiguration configration)
        {
            _context = context;
            _configration = configration;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository; 
        }
        public async Task<UserResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            return await RegisterWithRoleAsync(registerDto, registerDto.Role);

            //// Check if the user already exists
            //var existingUser = await _context.Users
            //    .FirstOrDefaultAsync(u => u.Email == registerDto.Email ||
            //                    u.Username == registerDto.Username);

            //if (existingUser != null)
            //{
            //    throw new InvalidOperationException("User with this email and username already exists");
            //}

            //// Hash the password
            //var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            //// Create new User
            //var user = new User
            //{
            //    Username = registerDto.Username,
            //    Email = registerDto.Email,
            //    PasswordHash = passwordHash,
            //    CreatedAt = DateTime.UtcNow
            //};

            //// Add to Database
            //_context.Users.Add(user);  // in memory insertion
            //await _context.SaveChangesAsync(); // persist to database

            ////Return response DTO(no password)
            //return new UserResponseDto
            //{
            //    Id= user.Id,
            //    UserName = user.Username,
            //    Email = user.Email,
            //    CreatedAt = user.CreatedAt
            //};
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto, string ipAddress)
        {
            // Find the user exist
            //var user = await _context.Users
            //     .Include(u=>u.Role)     // Eager load the role
            //     .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            // Find the user by email
            // UserRepository.GetByEmailAsync()
            // User base.FirstOrDefaultAsync() with the role eagerly loaded
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);

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

            //// Generate JWT Toke
            //var token = GenerateJwtToken(user);
            //var expireAt = DateTime.UtcNow.AddMinutes(double.Parse(_configration["JwtSettings:ExpiryMinutes"]!));

            // Generate Token
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken(ipAddress);

            // Save refresh token to database
            refreshToken.UserId = user.Id;

            // Uses base Generic AddAsync() from Repository<RefreshToken>
            await _refreshTokenRepository.AddAsync(refreshToken);

            //_context.RefreshTokens.Add(refreshToken);

            // Remove old/inactive refresh tokens for this user (optional cleanup)
            // await RemoveOldRefreshTokens(user.Id);
            // RefreshTokenRepository.RemoveOldTokenAsync()
            // Uses Base.RemoveRange() internally for cleanup
            await _refreshTokenRepository.RemoveOldTokenAsync(user.Id);

            //await _context.SaveChangesAsync();
            // Uses base generic SaveChangesAsync() from Repository<RefreshToken>
            await _refreshTokenRepository.SaveChangesAsync();

            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(
                double.Parse(_configration["JwtSettings:AccessTokenExpiryMinutes"]!));


            // Return login response
            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                User = new UserResponseDto
                {
                    Id = user.Id,
                    UserName= user.Username,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt,
                    Role = user.Role.Name
                },
                AccessTokenExpiresAt = accessTokenExpiry,
                RefreshTokenExpiresAt = refreshToken.ExpiresAt

            };
        }

        public async Task<UserResponseDto> RegisterWithRoleAsync(RegisterDto registerDto, string roleName)
        {
            // check if the user exist
            //var existingUser = await _context.Users
            //   .FirstOrDefaultAsync(u => u.Email == registerDto.Email ||
            //                          u.Username == registerDto.Username);

            // UserRepository.EmailExistAsync()
            // Uses base.AnyAsync() internally
            if (await _userRepository.EmailExistAsync(registerDto.Email))
            {
                throw new InvalidOperationException(
                     "User with this email already exists");
            }

            //if (existingUser != null)
            //{
            //    throw new InvalidOperationException("User with this email or username already exists");
            //}

            // UserRepository.UsernameExistsAsync()
            // Uses base.AnyAsync() internally
            if (await _userRepository.UsernameExistAsync(registerDto.Username))
            {
                throw new InvalidOperationException(
                     "User with this email already exists");
            }


            //// Get the role
            //var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            //if (role == null)
            //{
            //    // Default to User Role if specific role doesn't exits
            //    role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == RoleConstants.User);
            //    if (role == null)
            //    {
            //        throw new InvalidOperationException("Default User Role not found. Please ensure role are seen");
            //    }
            //}

            // Role lookup still uses _context directly
            // We don't have a RoleRepository - it's a simple lookup
            var role = await _context.Roles
                       .FirstOrDefaultAsync(r => r.Name == roleName)
                    ?? await _context.Roles
                       .FirstOrDefaultAsync(r => r.Name == RoleConstants.User)
                    ?? throw new InvalidOperationException(
                        "Default user role not found. Please ensure roles are seeded");


            // Hash the password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                RoleId = role.Id,   // Assign role
                CreatedAt = DateTime.UtcNow
            };

            // Add to database
            // Uses base generic AddAsync() from Repository<User>
            await _userRepository.AddAsync(user);

            // Uses base generic SaveChnagesAsync() from Repository<User>
            await _userRepository.SaveChangesAsync();

            //_context.Users.Add(user);
            //await _context.SaveChangesAsync();

            // Load the role navigation property
            //await _context.Entry(user).Reference(u => u.Role).LoadAsync();
            // UserRepository.GetUserWithRoleAsync()
            // Uses base.FirstOrDefaultAsync() with Role Include
            var userWithRole = await _userRepository.GetUserWithRoleAsync(user.Id);

            // Return response Dto
            return new UserResponseDto
            {
                Id = userWithRole!.Id,
                UserName = userWithRole.Username,
                Email = userWithRole.Email,
                CreatedAt = userWithRole.CreatedAt,
                Role = userWithRole.Role.Name     // Include role
            };
        }

        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(string token, string ipAddress)
        {
            //var user = await GetUserByRefreshToken(token);
            //var refreshToken = user.RefreshTokens.Single(rt => rt.Token == token);
            var user = await _userRepository.GetUserWithRefreshTokenAsync(token);

            if(user == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            var refreshToken = user.RefreshTokens.Single(rt => rt.Token == token);

            if (!refreshToken.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token"); 
            }

            // Generate new tokens,
            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken(ipAddress);
            newRefreshToken.UserId = user.Id;

            // Mark Old refresh token as replaced
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;

            // Uses base generic Update() from Repository<RefreshToken>
            _refreshTokenRepository.Update(refreshToken);

            // Save new Refresh Token
            //_context.RefreshTokens.Add(newRefreshToken);

            // Save new refresh token
            // Uses base generic AddAsync() from Repository<RefreshToken>
            await _refreshTokenRepository.AddAsync(newRefreshToken);

            // Remove Old Refresh Token
            //await RemoveOldRefreshTokens(user.Id);
            // RefreshTokenRepository.RemoveOldTokenAsync()
            // Uses base.RemoveRange() interally for cleanup
            await _refreshTokenRepository.SaveChangesAsync();

            //await _context.SaveChangesAsync();

            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(
                double.Parse(_configration["JwtSettings:AccessTokenExpiryMinutes"]!));

            return new RefreshTokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                AccessTokenExpiresAt = accessTokenExpiry,
                RefreshTokenExpiresAt = newRefreshToken.ExpiresAt
            };
        }

        public async Task<bool> RevokeTokenAsync(string token, string ipAddress)
        {
            //var user = await GetUserByRefreshToken(token);
            //var refreshToken = user.RefreshTokens.Single(rt => rt.Token == token);

            // UserRepository.GetUserWithRefreshTokenAsync()
            // Custom query - need User + RefreshTokens loaded
            var user = await _userRepository.GetUserWithRefreshTokenAsync(token);

            if(user == null)
            {
                return false;
            }

            var refreshToken = user.RefreshTokens.Single(rt => rt.Token == token);

            if (!refreshToken.IsActive)
            {
                return false;
            }

            // Revoke token
            refreshToken.RevokedAt = DateTime.Now;
            refreshToken.RevokedByIp = ipAddress;

            // Uses base generic Update() from Repository<RefreshToken>
            _refreshTokenRepository.Update(refreshToken);

            // Uses base generic SaveChnagesAsync() from Repository<RefreshToken>
            await _refreshTokenRepository.SaveChangesAsync();

            //await _context.SaveChangesAsync();

            return true;
        }

        //private string GenerateJwtToken(User user)
        //{
        //    //Get JWT setting from configuration
        //    var secretKey = _configration["JwtSettings:SecretKey"];
        //    var issuer = _configration["JwtSettings:Issuer"];
        //    var audience = _configration["JwtSettings:Audience"];
        //    var expiryMinutes = double.Parse(_configration["JwtSettings:ExpiryMinutes"]!);

        //    // Create Secret Key
        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        //    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    // Define claims (user information to include in token)
        //    var claims = new[]
        //    {
        //        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        //        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        //        new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
        //        new Claim(ClaimTypes.Role, user.Role.Name),     // Add role claim
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) 
        //    };

        //    // Create Token
        //    var token = new JwtSecurityToken(
        //        issuer: issuer,
        //        audience: audience,
        //        claims: claims,
        //        expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
        //        signingCredentials: credentials
        //        );

        //    // Return token as string
        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}


        private string GenerateAccessToken(User user)
        {
            //Get JWT setting from configuration
            var secretKey = _configration["JwtSettings:SecretKey"];
            var issuer = _configration["JwtSettings:Issuer"];
            var audience = _configration["JwtSettings:Audience"];
            var expiryMinutes = double.Parse(_configration["JwtSettings:AccessTokenExpiryMinutes"]!);

            // Create Secret Key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define claims (user information to include in token)
            var claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                    new Claim(ClaimTypes.Role, user.Role.Name),     // Add role claim
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

        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            var refreshTokenExpiry = double.Parse(_configration["JwtSettings:RefreshTokenExpiryDays"]!);

            // Generate cryptographically secure random token
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var token = Convert.ToBase64String(randomBytes);

            return new RefreshToken
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiry),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress,
            };
        }

        private async Task RemoveOldRefreshTokens(int userId)
        {
            // Remove refresh tokens that are expired or revoked and older than 30 days
            var tokensToRemove = await _context.RefreshTokens
                      .Where(rt => rt.UserId == userId &&
                                 // !rt.IsActive &&
                                 rt.RevokedAt != null &&
                                 rt.CreatedAt.AddDays(30) < DateTime.UtcNow)
                      .ToListAsync();

            if(tokensToRemove.Any())
            {
                _context.RefreshTokens.RemoveRange(tokensToRemove);
            }
        }

        private async Task<User> GetUserByRefreshToken(string token)
        {
            var user = await _context.Users
                      .Include(u => u.Role)
                      .Include(u => u.RefreshTokens)
                      .SingleOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == token));

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            return user;
        }
    }
}

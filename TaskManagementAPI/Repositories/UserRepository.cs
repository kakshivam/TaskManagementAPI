using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Model;

namespace TaskManagementAPI.Repositories
{
    /// <summary>
    /// User-specific repository implementation.
    ///
    /// FROM GENERIC REPOSITORY (inherited, no need to rewrite):
    ///   AddAsync()         → adds new user in AuthService.RegisterAsync()
    ///   SaveChangesAsync() → saves user in AuthService.RegisterAsync()
    ///   AnyAsync()         → building block for EmailExistsAsync and UsernameExistsAsync
    ///   FirstOrDefaultAsync() → building block for GetByEmailAsync, GetUserWithRoleAsync
    ///
    /// SPECIFIC TO THIS REPOSITORY (added here):
    ///   GetByEmailAsync()           → needs Role eager loading for JWT generation
    ///   EmailExistsAsync()          → user-friendly existence check
    ///   UsernameExistsAsync()       → user-friendly existence check
    ///   GetUserWithRoleAsync()      → needs Role eager loading
    ///   GetUserWithRefreshTokensAsync() → needs both Role and RefreshTokens loaded
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context): base(context) 
        { 
        }

        // --------------------------------------------------
        // USER-SPECIFIC IMPLEMENTATIONS
        // These use base class methods as building blocks
        // --------------------------------------------------

        /// <summary>
        /// Gets user by email with Role eagerly loaded.
        /// Role is always needed in AuthService for JWT token generation.
        /// Uses base.FirstOrDefaultAsync() with case-insensitive comparison.
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            // Include Role because AuthService needs user.Role.Name for JWT Claims
            return await _dbSet
                .Include(u => u.Role)  // Eager load Role for JWT Generation 
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        /// <summary>
        /// Checks email uniqueness during registration.
        /// More efficient than GetByEmail - stops as soon as match is found.
        /// Uses base.AnyAsync() as building block.
        /// </summary>
        public async Task<bool> EmailExistAsync(string email)
        {
            // Uses base generic AnyAsync - no need to rewrite the DbSet logic
            return await AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        /// <summary>
        /// Checks username uniqueness during registration.
        /// More efficient than GetByUsername - stops as soon as match is found.
        /// Uses base.AnyAsync() as building block.
        /// </summary>
        public async Task<bool> UsernameExistAsync(string email)
        {
            // Uses base generic AnyAsync - no need to rewrite the DbSet logic
            return await AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        /// <summary>
        /// Gets user with Role loaded after registration.
        /// Role is needed to include in the registration response.
        /// Uses base.FirstOrDefaultAsync() with Role included.
        /// </summary>
        public async Task<User?> GetUserWithRoleAsync(int userId)
        {
            // Include Role because we need user.Role.Name in UserResponseDto
            return await _dbSet
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        /// <summary>
        /// Gets user with both Role AND RefreshTokens loaded.
        /// Needed for token refresh and revocation operations.
        /// Cannot use base.FirstOrDefaultAsync() because we need multiple Includes.
        /// </summary>
        public async Task<User?> GetUserWithRefreshTokenAsync(string refreshToken)
        {
            // Must write custom query here because we need TWO levels of eager loading:
            // 1. Include Role      → needed for JWT generation when refreshing
            // 2. Include RefreshTokens → needed to find and validate the specific token
            return await _dbSet
                .Include(u => u.Role)
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.RefreshTokens
                .Any(rt => rt.Token == refreshToken));
        }
    }
}

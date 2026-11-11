using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Model;

namespace TaskManagementAPI.Repositories
{
    /// <summary>
    /// RefreshToken-specific repository implementation.
    ///
    /// FROM GENERIC REPOSITORY (inherited, no need to rewrite):
    ///   AddAsync()         → used in AuthService.LoginAsync() to save new token
    ///   Update()           → used in AuthService to mark old tokens as revoked/replaced
    ///   RemoveRange()      → building block for RemoveOldTokensAsync()
    ///   SaveChangesAsync() → used in AuthService after every token operation
    ///
    /// SPECIFIC TO THIS REPOSITORY (added here):
    ///   GetByTokenAsync()    → needs User + Role eager loading for JWT generation
    ///   RemoveOldTokensAsync() → token-specific cleanup logic
    /// </summary>
    public class RefreshTokenRepository: Repository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext context): base(context)
        {
        }

        // --------------------------------------------------
        // REFRESH TOKEN-SPECIFIC IMPLEMENTATIONS
        // --------------------------------------------------

        /// <summary>
        /// Gets a refresh token with full User context loaded.
        /// Used in AuthService for both RefreshTokenAsync and RevokeTokenAsync.
        /// Cannot use base.FirstOrDefaultAsync() because we need:
        /// - User included → to generate new JWT access token
        /// - User.Role included → to include role claim in new JWT
        /// </summary>
        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            // Must write custom query here because we need nested eager loading:
            // 1. Include User         → we need user details for new JWT
            // 2. ThenInclude Role     → we need role name for JWT claims
            // This cannot be achieved with base.FirstOrDefaultAsync()
            return await _dbSet
                .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        /// <summary>
        /// Removes tokens that are inactive AND older than specified days.
        /// Called on every login and refresh to keep database clean.
        /// Uses base.RemoveRange() as building block.
        /// </summary>
        public async Task RemoveOldTokenAsync(int userId, int daysOld=30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);

            // Find tokens that are:
            // 1. Belong to this user
            // 2. Are inactive (expired OR revoked)
            // 3. Were created more than 'daysOld' days ago
            var oldTokens = await _dbSet
                .Where(rt => rt.UserId == userId &&
                             (rt.RevokedAt != null ||
                              rt.ExpiresAt < DateTime.UtcNow) &&
                              rt.CreatedAt < cutoffDate)
                .ToListAsync();

            if(oldTokens.Any())
            {
                // Uses base generic RemoveRange - avoids rewriting DbSet.RemoveRange
                RemoveRange(oldTokens);
                await SaveChangesAsync();
            }
        }
    }
}

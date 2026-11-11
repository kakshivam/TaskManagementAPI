using TaskManagementAPI.Model;

namespace TaskManagementAPI.Repositories
{
    /// <summary>
    /// RefreshToken-specific repository.
    /// Inherits from IRepository<RefreshToken> which provides:
    ///   - AddAsync()         → used in AuthService.LoginAsync() to save new refresh token
    ///   - Update()           → used in AuthService to mark tokens as revoked/replaced
    ///   - RemoveRange()      → used in RemoveOldTokensAsync() for cleanup
    ///   - SaveChangesAsync() → used in AuthService after token operations
    /// This interface adds refresh token-specific operations on top.
    /// </summary>
    public interface IRefreshTokenRepository: IRepository<RefreshToken>
    {
        // --------------------------------------------------
        // REFRESH TOKEN-SPECIFIC OPERATIONS
        // These are additional to what IRepository<RefreshToken> provides
        // --------------------------------------------------

        /// <summary>
        /// Gets a refresh token by its token string with User and Role loaded.
        /// Used in: AuthService.RefreshTokenAsync(), AuthService.RevokeTokenAsync()
        /// Requires eager loading because we need user.Role.Name for new JWT
        /// </summary>
        Task<RefreshToken?> GetByTokenAsync(string token);

        /// <summary>
        /// Removes expired and revoked tokens older than specified days.
        /// Used in: AuthService.LoginAsync() and AuthService.RefreshTokenAsync()
        /// Keeps database clean - prevents token table from growing indefinitely
        /// Internally uses: base.RemoveRange()
        /// </summary>
        Task RemoveOldTokenAsync(int userId, int daysOld = 30);
    }
}

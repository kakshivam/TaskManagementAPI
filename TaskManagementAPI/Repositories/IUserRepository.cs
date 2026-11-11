using TaskManagementAPI.Model;

namespace TaskManagementAPI.Repositories
{
    /// <summary>
    /// User-specific repository.
    /// Inherits from IRepository<User> which provides:
    ///   - AddAsync()        → used in AuthService.RegisterAsync()
    ///   - SaveChangesAsync() → used in AuthService.RegisterAsync()
    /// This interface adds user-specific queries on top.
    /// </summary>
    public interface IUserRepository: IRepository<User>
    {
        // --------------------------------------------------
        // USER-SPECIFIC QUERIES
        // These are additional to what IRepository<User> provides
        // --------------------------------------------------

        /// <summary>
        /// Gets user by email with Role loaded.
        /// Used in: AuthService.LoginAsync()
        /// Internally uses: base.FirstOrDefaultAsync()
        /// </summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Checks if email is already registered.
        /// Used in: AuthService.RegisterWithRoleAsync()
        /// Internally uses: base.AnyAsync()
        /// </summary>
        Task<bool> EmailExistAsync(string email);

        /// <summary>
        /// Checks if username is already taken.
        /// Used in: AuthService.RegisterWithRoleAsync()
        /// Internally uses: base.AnyAsync()
        /// </summary>
        Task<bool> UsernameExistAsync(string username);

        /// <summary>
        /// Gets user with Role loaded.
        /// Used in: AuthService.RegisterWithRoleAsync() after saving new user
        /// Internally uses: base.FirstOrDefaultAsync()
        /// </summary>
        Task<User?> GetUserWithRoleAsync(int userId);

        /// <summary>
        /// Gets user with Role and RefreshTokens loaded.
        /// Used in: AuthService.RefreshTokenAsync(), AuthService.RevokeTokenAsync()
        /// Requires eager loading of both Role and RefreshTokens navigation properties
        /// </summary>
        Task<User?> GetUserWithRefreshTokenAsync(string refreshToken);
    }
}

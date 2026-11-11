using System.Linq.Expressions;

namespace TaskManagementAPI.Repositories
{
    /// <summary>
    /// Generic repository interface providing common data access operations.
    /// These methods are available to ALL specific repositories through inheritance.
    /// Used directly in services for: Add, Update, Remove, SaveChanges, GetById
    /// Used as building blocks in specific repositories for: AnyAsync, FirstOrDefaultAsync, FindAsync
    /// </summary>
    public interface IRepository<T> where T : class
    {
        // --------------------------------------------------
        // QUERY METHODS
        // Used as building blocks in specific repositories
        // --------------------------------------------------

        /// <summary>
        /// Gets a single entity by primary key.
        /// Used in: TaskService.GetAnyTaskByIdAsync(), TaskService.UpdateAnyTaskAsync()
        /// </summary>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Gets first entity matching condition or null.
        /// Used as building block in: UserRepository, RefreshTokenRepository
        /// </summary>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T,bool>> predicate);

        /// <summary>
        /// Checks if any entity matches the condition.
        /// Used as building block in: UserRepository.EmailExistsAsync(), UsernameExistsAsync()
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Counts entities matching a condition.
        /// Used as building block in: TaskRepository.GetUserTaskCountAsync()
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);

        // --------------------------------------------------
        // COMMAND METHODS
        // Used directly in services for all entities
        // --------------------------------------------------

        /// <summary>
        /// Adds a new entity to the database.
        /// Used in: AuthService (add user), AuthService (add refresh token), TaskService (add task)
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Marks an entity as modified.
        /// Used in: TaskService (update task), AuthService (update refresh token)
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Marks an entity for deletion.
        /// Used in: TaskService (delete task)
        /// </summary>
        void Remove(T entity);

        /// <summary>
        /// Marks multiple entities for deletion.
        /// Used in: RefreshTokenRepository.RemoveOldTokensAsync()
        /// </summary>
        void RemoveRange(IEnumerable<T> entities);

        // --------------------------------------------------
        // PERSISTENCE
        // Used in every service after data modifications
        // --------------------------------------------------

        /// <summary>
        /// Saves all pending changes to the database.
        /// Used in: AuthService, TaskService after every data modification
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}

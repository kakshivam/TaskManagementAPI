using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskManagementAPI.Data;

namespace TaskManagementAPI.Repositories
{
    /// <summary>
    /// Generic repository implementation.
    /// All specific repositories (UserRepository, TaskRepository, RefreshTokenRepository)
    /// inherit from this class and get these methods for free.
    /// Specific repositories only add entity-specific queries on top.
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // --------------------------------------------------
        // QUERY IMPLEMENTATIONS
        // --------------------------------------------------

        // Used directly in TaskService for admin operations 
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        // Used as buiding block inside specific repositories
        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        // Used as buiding block inside specific repositories
        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        // Used as buiding block inside specific repositories
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        // --------------------------------------------------
        // COMMAND IMPLEMENTATIONS
        // --------------------------------------------------

        // Used directly in AuthService and TaskService
        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        // Used directly in TaskService and AuthService
        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        // Used in TaskService
        public virtual void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        // Used in RefreshTokenRepository.RemoveOldTokenAsync()
        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        // --------------------------------------------------
        // PERSISTENCE
        // --------------------------------------------------

        // Used in every service after data modification
        public virtual async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Model;

namespace TaskManagementAPI.Repositories
{
    public class TaskRepository: Repository<TaskItem>, ITaskRepository
    {
        
        public TaskRepository(ApplicationDbContext context): base(context)
        { 
        }

        // --------------------------------------------------
        // TASK-SPECIFIC IMPLEMENTATIONS
        // --------------------------------------------------

        /// <summary>
        /// Gets all tasks for a specific user ordered by newest first.
        /// Regular users call this through TaskService.GetUserTasksAsync().
        /// UserId filter is CRITICAL here - ensures data isolation.
        /// Cannot use base.FindAsync() because we need specific ordering.
        /// </summary>
        public async Task<IEnumerable<TaskItem>> GetUserTaskAsync(int userId)
        {
            return await _dbSet
                .Where(t => t.UserId ==  userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Gets ALL tasks in the system for admin/manager access.
        /// No userId filter - this is intentional for admin/manager role.
        /// Only called from TaskService methods that have role-based authorization.
        /// Cannot use base.GetAllAsync() because we need specific ordering.
        /// </summary>
        public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
        {
            return await _dbSet
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync ();    
        }

        /// <summary>
        /// Gets a single task ONLY if it belongs to the specified user.
        /// This is the primary security enforcement method.
        /// Used for: get single, update, delete, toggle operations.
        /// Cannot use base.GetByIdAsync() because that has no userId filter.
        /// </summary>
        public async Task<TaskItem?> GetUserTaskByIdAsync(int taskId,int userId)
        {
            // Both conditions required:
            // t.Id == taskId    → find the specific task
            // t.UserId == userId → ensure it belongs to the requesting user
            // If either fails, returns null → controller returns 404
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
        }

        /// <summary>
        /// Gets the total number of tasks for a user.
        /// Used in AdminController for user statistics.
        /// Uses base.CountAsync() as building block - no need to rewrite.
        /// </summary>
        public async Task<int> GetUserTaskCountAsync(int userId)
        {
            // Uses base generic CountAsync - avoids rewriting DbSet.CountAsync
            return await CountAsync(t => t.UserId == userId);
        }

    }
}

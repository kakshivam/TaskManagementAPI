using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Reflection;
using TaskManagementAPI.Data;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Model;
using TaskManagementAPI.Repositories;

namespace TaskManagementAPI.Services
{
    public class TaskService: ITaskService
    {
        // TaskRepository replaces all direct DbContext usage for task operation
        // All data access now goes through the repository
        private readonly ITaskRepository _taskRepository;
        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto, int userId)
        {
            // Create new task entity
            var task = new TaskItem
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                Priority = createTaskDto.Priority,
                DueDate = createTaskDto.DueDate,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UserId = userId    // Associate with current user
            };

            // Add to Database
            //_context.Tasks.Add(task);
            //await _context.SaveChangesAsync();

            // Uses base generic AddAsync() from Repository<TaskItem>
            await _taskRepository.AddAsync(task);

            // Uses base generic SaveChangesAsync() from Repository<TaskItem>
            await _taskRepository.SaveChangesAsync();

            // return response DTO
            return MapToResponseDto(task);
        }

        public async Task<List<TaskResponseDto>> GetUserTaskAsync(int userId)
        {
            // Get all tasks for this user, ordered by creation date (newest first)
            //var tasks = await _context.Tasks
            //    .Where(t => t.UserId == userId)
            //    .OrderByDescending(t => t.CreatedAt)
            //    .Select(t => new TaskResponseDto
            //    {
            //        Id = t.Id,
            //        Title= t.Title,
            //        Description =t.Description,
            //        IsCompleted = t.IsCompleted,
            //        Priority = t.Priority,
            //        DueDate =t.DueDate,
            //        CreatedAt = t.CreatedAt,
            //        UserId = t.UserId
            //    })
            //    .ToListAsync();

            //return tasks;

            // TaskRepository.GetUserTaskAsync()
            // Custom query - needs userId filter + ordering
            var tasks = await _taskRepository.GetUserTaskAsync(userId);
            return tasks.Select(MapToResponseDto).ToList();
        }

        public async Task<TaskResponseDto?> GetTaskByIdAsync(int taskId, int userId)
        {
            // Find the task by Id and ensure it belong to user
            //var task = await _context.Tasks
            //    .Where(t => t.Id == taskId && t.UserId == userId)
            //    .Select(t => new TaskResponseDto
            //    {
            //        Id = t.Id,
            //        Title = t.Title,
            //        Description = t.Description,
            //        IsCompleted = t.IsCompleted,
            //        Priority = t.Priority,
            //        DueDate = t.DueDate,
            //        CreatedAt = t.CreatedAt,
            //        UserId = t.UserId
            //    })
            //    .FirstOrDefaultAsync();

            // return task;

            var tasks = await _taskRepository.GetUserTaskByIdAsync(taskId, userId);
            return tasks == null ? null : MapToResponseDto(tasks);
        }

        public async Task<TaskResponseDto?> UpdateTaskAsync(int taskId, UpdateTaskDto updateTaskDto, int userId)
        {
            // Find the task and verify ownership 
            //var task = await _context.Tasks
            //    .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

            // TaskRepository.GetUserTaskByIdAsync()
            // Security check: ensures that task belongs to requesting user
            var task = await _taskRepository.GetUserTaskByIdAsync(taskId, userId);

            if(task == null)
            {
                return null; // Task not found or user doesn't own it
            }

            // Update only field that were provided (not null)
            ApplyUpdate(task, updateTaskDto);

            // Save changes
            // Uses base generic Update() from Repository<TaskItem>
            _taskRepository.Update(task);

            // Uses base generic SaveChangesAsync() from Repository<TaskItem>
            await _taskRepository.SaveChangesAsync();

            // Return updated tasks
            return MapToResponseDto(task);
        }

        public async Task<bool> DeleteTaskAsync(int taskId,int userId)
        {
            // Fint the task and verify ownership
            //var task = await _context.Tasks
            //.FirstOrDefaultAsync(t=> t.Id == taskId && t.UserId == userId);
            // TaskRepository.GetUserTaskByIdAsync()
            // Security check: enusres task belongs to requesting user
            var task = await _taskRepository.GetUserTaskByIdAsync(taskId, userId);

            if (task != null)
            {
                return false;  // Task not found or user doesn't own it
            }

            // Remove the task
            // Uses base generic Remove() from Reposiotry<TaskItem>
            _taskRepository.Remove(task);

            // Uses base generic SaveChangesAsync() from Repository<TaskItem>
            await _taskRepository.SaveChangesAsync();

            return true;
        }

        public async Task<TaskResponseDto?> ToggleTaskCompletionAsync(int taskId, int userId)
        {
            // Find the task and verify the ownership
            //var task = await _context.Tasks
            //    .FirstOrDefaultAsync(t=> t.Id == taskId && t.UserId == userId);

            // TaskRepository.GetUserTaskByIdAsync()
            // Security check: ensures tasks belong to requesting user
            var task = await _taskRepository.GetUserTaskByIdAsync(taskId, userId);

            if(task != null)
            {
                return null;
            }

            // Toggle the completion status
            task.IsCompleted = !task.IsCompleted;

            // Save changes
            // Uses base generic Update() from Repository<TaskItem>
            _taskRepository.Update(task);

            // Uses base generic SaveChangesAsync() from Repository<TaskItem>
            await _taskRepository.SaveChangesAsync();

            // return updated task
            return MapToResponseDto(task);
        }

        public async Task<List<TaskResponseDto>> GetAllTasksAsync()
        {
            //var tasks = await _context.Tasks
            //    .OrderByDescending(t => t.CreatedAt)
            //    .Select(t => new TaskResponseDto
            //         {
            //           Id = t.Id,
            //           Title = t.Title,
            //           Description = t.Description,
            //           IsCompleted = t.IsCompleted,
            //           Priority = t.Priority,
            //           DueDate = t.DueDate,
            //           CreatedAt = t.CreatedAt,
            //           UserId = t.UserId
            //        })
            //   .ToListAsync();

            //return (tasks);

            // TaskRepository.GetAllTaskAsync()
            // Custom Query - no usedId filter (admin/manager access only)
            var tasks = await _taskRepository.GetAllTasksAsync();
            return tasks.Select(MapToResponseDto).ToList();
        }

        public async Task<TaskResponseDto?> GetAnyTaskByIdAsync(int taskId)
        {
            //var tasks = await _context.Tasks
            //    .Where(t => t.Id == taskId)
            //    .Select(t => new TaskResponseDto
            //    {
            //        Id = t.Id,
            //        Title = t.Title,
            //        Description = t.Description,
            //        IsCompleted = t.IsCompleted,
            //        Priority = t.Priority,
            //        DueDate = t.DueDate,
            //        CreatedAt = t.CreatedAt,
            //        UserId = t.UserId
            //    })
            //    .FirstOrDefaultAsync();

            //return (tasks);

            // Uses base generic GetByIdAsync() from Repository<TaskItem>
            // No User Id filter needed - admin operation
            var task = await _taskRepository.GetByIdAsync(taskId);
            return task == null ? null : MapToResponseDto(task);
        }

        public async Task<TaskResponseDto?> UpdateAnyTaskAsync(int taskId, UpdateTaskDto updateTaskDto)
        {
            // Find the tasks without filtering the userId
            //var task = await _context.Tasks
            //          .FirstOrDefaultAsync(t => t.Id == taskId);
            // Uses base generic GetByIdAsync() from Repository<TaskItem>
            // No usedId check - admin operation
            var task = await _taskRepository.GetByIdAsync(taskId);

            if (task == null)
            {
                return null;
            }

            // Update Fields
            ApplyUpdate(task, updateTaskDto);

            // Uses base generic Update() from Repository<TaskItem>
            _taskRepository.Update(task);

            // Uses base generic SaveChnagesAsync() from Repository<TaskItem>
            await _taskRepository.SaveChangesAsync();

            return MapToResponseDto(task);
        }

        public async Task<bool> DeleteAnyTaskAsync(int taskId)
        {
            //var task = await _context.Tasks
            //    .FirstOrDefaultAsync(t => t.Id == taskId);
            // Uses base Generic GetByIdAsync() from Repository<TaskItem>
            // No userId check - admin operation
            var task = await _taskRepository.GetByIdAsync(taskId);

            if(task == null)
            {
                return false;
            }

            //_context.Tasks.Remove(task);
            //await _context.SaveChangesAsync

            // Uses base generic Remove() from Repository<TaskItem>
            _taskRepository.Remove(task);

            // Uses base generic SaveChnagesAsync() from Repository<TaskItem>
            await _taskRepository.SaveChangesAsync();

            return true;
        }

        private TaskResponseDto MapToResponseDto(TaskItem task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                IsCompleted = task.IsCompleted,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                UserId = task.UserId
            };
        }

        private void ApplyUpdate(TaskItem task, UpdateTaskDto updateTaskDto)
        {
            if (updateTaskDto.Title != null)
            {
                task.Title = updateTaskDto.Title;
            }

            if (updateTaskDto.Description != null)
            {
                task.Description = updateTaskDto.Description;
            }

            if (updateTaskDto.IsCompleted.HasValue)
            {
                task.IsCompleted = updateTaskDto.IsCompleted.Value;
            }

            if (updateTaskDto.Priority.HasValue)
            {
                task.Priority = updateTaskDto.Priority.Value;
            }

            if (updateTaskDto.DueDate.HasValue)
            {
                task.DueDate = updateTaskDto.DueDate.Value;
            }
        }
    }
}

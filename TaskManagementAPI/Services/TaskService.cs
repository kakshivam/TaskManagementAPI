using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Reflection;
using TaskManagementAPI.Data;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Model;

namespace TaskManagementAPI.Services
{
    public class TaskService: ITaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
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
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // return response DTO
            return new TaskResponseDto
            {
                Id= task.Id,
                Title = task.Title,
                Description= task.Description,
                IsCompleted= task.IsCompleted,
                Priority = task.Priority,
                DueDate= task.DueDate,
                CreatedAt= task.CreatedAt,
                UserId = task.UserId
            };
        }

        public async Task<List<TaskResponseDto>> GetUserTaskAsync(int userId)
        {
            // Get all tasks for this user, ordered by creation date (newest first)
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title= t.Title,
                    Description =t.Description,
                    IsCompleted = t.IsCompleted,
                    Priority = t.Priority,
                    DueDate =t.DueDate,
                    CreatedAt = t.CreatedAt,
                    UserId = t.UserId
                })
                .ToListAsync();

            return tasks;
        }

        public async Task<TaskResponseDto?> GetTaskByIdAsync(int taskId, int userId)
        {
            // Find the task by Id and ensure it belong to user
            var task = await _context.Tasks
                .Where(t => t.Id == taskId && t.UserId == userId)
                .Select(t => new TaskResponseDto
                {
                    Id= t.Id,
                    Title = t.Title,
                    Description= t.Description,
                    IsCompleted = t.IsCompleted,
                    Priority = t.Priority,
                    DueDate =t.DueDate,
                    CreatedAt = t.CreatedAt,
                    UserId = t.UserId
                })
                .FirstOrDefaultAsync();

            return task;
        }

        public async Task<TaskResponseDto?> UpdateTaskAsync(int taskId, UpdateTaskDto updateTaskDto, int userId)
        {
            // Find the task and verify ownership 
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

            if(task == null)
            {
                return null; // Task not found or user doesn't own it
            }

            // Update only field that were provided (not null)
            if(updateTaskDto.Title != null)
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

            if(updateTaskDto.DueDate.HasValue)
            {
                task.DueDate = updateTaskDto.DueDate.Value;
            }

            // Save changes
            await _context.SaveChangesAsync();

            // Return updated tasks
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

        public async Task<bool> DeleteTaskAsync(int taskId,int userId)
        {
            // Fint the task and verify ownership
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t=> t.Id == taskId && t.UserId == userId);

            if (task != null)
            {
                return false;  // Task not found or user doesn't own it
            }

            // Remove the task
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<TaskResponseDto?> ToggleTaskCompletionAsync(int taskId, int userId)
        {
            // Find the task and verify the ownership
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t=> t.Id == taskId && t.UserId == userId);

            if(task != null)
            {
                return null;
            }

            // Toggle the completion status
            task.IsCompleted = !task.IsCompleted;

            // Save changes
            await _context.SaveChangesAsync();

            // return updated task
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description= task.Description,
                IsCompleted= task.IsCompleted,
                Priority = task.Priority,
                DueDate= task.DueDate,
                CreatedAt = task.CreatedAt,
                UserId = task.UserId
            };

        }
    }
}

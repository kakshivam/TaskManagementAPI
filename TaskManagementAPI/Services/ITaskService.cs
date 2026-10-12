using TaskManagementAPI.DTOs;

namespace TaskManagementAPI.Services
{
    public interface ITaskService
    {
        Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto, int userId);
        Task<List<TaskResponseDto>> GetUserTaskAsync(int userId);
        Task<TaskResponseDto?> GetTaskByIdAsync(int taskId, int userId);
        Task<TaskResponseDto?> UpdateTaskAsync(int taskId, UpdateTaskDto updateTaskDto, int userId);
        Task<bool> DeleteTaskAsync(int taskId, int userId);
        Task<TaskResponseDto?> ToggleTaskCompletionAsync(int taskId, int userId);

        // New Admin/Manager Method
        Task<List<TaskResponseDto>> GetAllTasksAsync();  // For manager and admin
        Task<TaskResponseDto?> GetAnyTaskByIdAsync(int taskId);   // Get Any Task regardless of owner
        Task<TaskResponseDto?> UpdateAnyTaskAsync(int taskId, UpdateTaskDto updateTaskDto);   // Update any Task
        Task<bool> DeleteAnyTaskAsync(int taskId);   // Delete any task
    }
}

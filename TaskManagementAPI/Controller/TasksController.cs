using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Extensions;
using TaskManagementAPI.Services;
using TaskManagementAPI.Model;
using TaskManagementAPI.Constants;

namespace TaskManagementAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // This entire controller requires authentication!
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        // Regular user, Manager, and Admin can create tasks
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
        {
            try
            {
                // Get the current user Id from Jwt token
                var userId = User.GetUserId();

                // Create the task
                var task = await _taskService.CreateTaskAsync(createTaskDto, userId);

                return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occured while creating the task", details = ex.Message });
            }
        }

        // Regular user see only there tasks
        // Manager and Admin can see there tasks
        [HttpGet]
        public async Task<IActionResult> GetUserTasks()
        {
            try
            {
                // Get the current user Id from Jwt Toke
                var userId = User.GetUserId();

                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                List<TaskResponseDto> tasks;

                //Manager and Admins can see all tasks
                if(userRole == RoleConstants.Manager || userRole == RoleConstants.Admin)
                {
                    tasks = await _taskService.GetAllTasksAsync(); 
                }
                else
                {
                    tasks = await _taskService.GetUserTaskAsync(userId);
                }

                return Ok(tasks);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occured while retreving tasks", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            try
            {
                // Get the current user Id from Jwt Token
                var user = User.GetUserId();
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                // Get the task (includes Authorization check)
                var task = await _taskService.GetTaskByIdAsync(id, user);

                if (task == null)
                 {
                    // Manager and Admin can view any task
                    if(userRole == RoleConstants.Manager || userRole == RoleConstants.Admin)
                    {
                        task = await _taskService.GetAnyTaskByIdAsync(id);
                    }
                    if (task == null)
                    {
                        return NotFound(new { message = "Task not found or you don't have permission to find the task" });
                    }
                }

                return Ok(task);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occured while retreving the task", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
        {
            try
            {
                var userId = User.GetUserId();
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                // Only Admins can update other users tasks
                TaskResponseDto? task;

                if(userRole == RoleConstants.Admin)
                {
                    task = await _taskService.UpdateAnyTaskAsync(id, updateTaskDto);
                }
                else
                {
                    task = await _taskService.UpdateTaskAsync(id, updateTaskDto, userId);
                }

                if(task == null)
                {
                    return NotFound(new { message = "Task not found or you don't have permission to update it" });
                }

                return Ok(task);
            }
            catch(UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occured while updating the task", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy ="AdminOnly")]                    // Explicit admin-only authorization
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var userId = User.GetUserId();
                //var success = await _taskService.DeleteTaskAsync(id, userId);
                var success = await _taskService.DeleteAnyTaskAsync(id);

                if (!success)
                {
                    return NotFound(new { message = "Task not found or you don't have permission to delete it" });
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occured while deleting the task", details = ex.Message });
            }
        }

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleTaskCompletion(int id)
        {
            try
            {
                var userId = User.GetUserId();
                var task = await _taskService.ToggleTaskCompletionAsync(id, userId);

                if(task != null)
                {
                    return NotFound(new { message = "Task not found or you don't have permission to modify it" });
                }

                return Ok(task);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occured while deleting the task", details = ex.Message });
            }
        }

    }
}

using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.DTOs
{
    public class ChangeRoleDto
    {
        [Required(ErrorMessage = "Role name is required")]
        public string RoleName { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage="Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage="Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage ="Email is required")]
        [EmailAddress(ErrorMessage ="Invalid Email Format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage ="Password is Required")]
        [StringLength(100, MinimumLength =6, ErrorMessage ="Password must be atleast 6 characters")]
        public string Password {  get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage="Email is Rquired")]
        [EmailAddress(ErrorMessage ="Invalid Email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage ="Password is Required")]
        public string Password { get; set; }= string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.DTOs
{
    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "Refresh Token is Required")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}

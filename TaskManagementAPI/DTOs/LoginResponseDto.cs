namespace TaskManagementAPI.DTOs
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;     // Renamed From Token
        public string RefreshToken {  get; set; } = string.Empty;   // New Refresh Token
        public UserResponseDto User { get; set; } = null!;
        public DateTime AccessTokenExpiresAt { get; set; }          // Renamed from ExpiresAt
        public DateTime RefreshTokenExpiresAt { get; set; }         // New 
    }
}

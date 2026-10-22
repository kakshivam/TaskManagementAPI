namespace TaskManagementAPI.Model
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;   //The actual refresh token string
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime RevokedAt { get; set; }    // Null if still active
        public string? RevokedByIp { get; set; }   // IP that revoked it
        public string? ReplacedByToken { get; set; }  // Token that replaced this one (for rotation)
        public string CreatedByIp { get; set; } = string.Empty;   // IP that created it

        // Foregin Key
        public int UserId { get; set; }

        // Navigation property
        public User User { get; set; } = null!;

        // Helper properties
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt != null;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}

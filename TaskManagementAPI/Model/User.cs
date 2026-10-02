namespace TaskManagementAPI.Model
{
    public class User
    {
        public int Id {  get; set; } 
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int RoleId { get; set; }    // Foregin Key
        public Role Role { get; set; } = null!;  // Navigation Property

        // Navigation Property
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}

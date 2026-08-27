namespace TaskManagementAPI.Model
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
        public int Priority { get; set; } = 1;   // Low=1, Medium=2, High=3
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foregin Key
        public int UserId { get; set; }

        // Navigation property
        public User User { get; set; } = null!;
    }
}

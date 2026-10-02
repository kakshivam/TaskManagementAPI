namespace TaskManagementAPI.Model
{
    public class Role
    {
        public int Id {  get; set; }
        public string Name { get; set; } = string.Empty;  // Admin, Manager, User
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Navigation Propoerty
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}

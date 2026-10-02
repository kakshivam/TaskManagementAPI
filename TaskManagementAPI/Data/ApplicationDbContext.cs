using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Model;

namespace TaskManagementAPI.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Role> Roles {  get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User-Task Relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.Tasks)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Role-User Relationships
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);    // Don't allow deleting role if user exist

            // Configure Index for better performance
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();    // Role names must be unique

            //modelBuilder.Entity<Role>()
            //    .Property(r => r.CreatedAt)
            //    .HasDefaultValueSql("GETUTCDATE()");

            // See the default role
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = 1,
                    Name = "Admin",
                    Description = "Full System access - can manage user and all tasks",
                    CreatedAt = new DateTime(2026, 10, 2, 0, 0, 0, DateTimeKind.Utc)
                },
                new Role
                {
                    Id = 2,
                    Name = "Manager",
                    Description = "Can view all tasks but modify own tasks",
                    CreatedAt = new DateTime(2026, 10, 2, 0, 0, 0, DateTimeKind.Utc)
                },
                new Role
                {
                    Id = 3,
                    Name = "User",
                    Description = "Can only manage own tasks",
                    CreatedAt = new DateTime(2026, 10, 2, 0, 0, 0, DateTimeKind.Utc)
                }
                );
        }

    }
}

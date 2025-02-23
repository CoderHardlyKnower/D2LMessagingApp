using Microsoft.EntityFrameworkCore;
using MessagingApp.Models;

namespace MessagingApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define a composite primary key for Enrollment
            modelBuilder.Entity<Enrollment>().HasKey(e => new { e.UserId, e.CourseId });

            // Configure the Course - CourseInstructor relationship using a shadow foreign key.
            // This ensures that deleting a User (instructor) does not cascade and delete Courses.
            modelBuilder.Entity<Course>()
                .HasOne(c => c.CourseInstructor)
                .WithMany() // No navigation property on User for courses they instruct
                .HasForeignKey("CourseInstructorUserId")
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

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
         public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define a composite primary key for Enrollment
            modelBuilder.Entity<Enrollment>().HasKey(e => new { e.UserId, e.CourseId });

            // Configure the Course - CourseInstructor relationship using a shadow foreign key.
            modelBuilder.Entity<Course>()
                .HasOne(c => c.CourseInstructor)
                .WithMany() 
                .HasForeignKey("InstructorId")
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Conversation entity to generate the ConversationId on add
            modelBuilder.Entity<Conversation>()
                .Property(c => c.ConversationId)
                .ValueGeneratedOnAdd();

            // Define ConversationParticipant composite key
            modelBuilder.Entity<ConversationParticipant>()
                .HasKey(cp => new { cp.ConversationId, cp.UserId });

            modelBuilder.Entity<Conversation>()
                .HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConversationParticipant>()
                .HasOne(cp => cp.Conversation)
                .WithMany(c => c.Participants)
                .HasForeignKey(cp => cp.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConversationParticipant>()
                .HasOne(cp => cp.User)
                .WithMany() 
                .HasForeignKey(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

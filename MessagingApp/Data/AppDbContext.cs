﻿using Microsoft.EntityFrameworkCore;
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

        }
    }
}

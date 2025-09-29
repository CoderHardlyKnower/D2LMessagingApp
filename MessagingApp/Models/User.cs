using Microsoft.EntityFrameworkCore;

namespace MessagingApp.Models
{
    [Index(nameof(Email), IsUnique = false)]
    [Index(nameof(ExternalObjectId), IsUnique = false)]
    public class User
    {
        public int UserId { get; set; }

        // The full name of the user.
        public string Name { get; set; } = string.Empty;

        // The user's email, which serves as the login username
        public string Email { get; set; } = string.Empty;
        // Kept for backward compatibility; not used by Entra auth
        public string Password { get; set; } = string.Empty;
        // User type: "student" or "instructor" (potential future use)
        public string? UserType { get; set; }
        public string? ExternalObjectId { get; set; }

        // NEW: single source of truth for how we show names in the app
        public string DisplayName { get; set; } = "User";

        public List<Enrollment>? Enrollments { get; set; }

        public User() { }

        public User(string name)
        {
            Name = name;
            DisplayName = string.IsNullOrWhiteSpace(name) ? "User" : name;
        }

        public User(string name, string email, string password, string? userType = null)
        {
            Name = name;
            Email = email;
            Password = password;
            UserType = userType;
            DisplayName = string.IsNullOrWhiteSpace(name) ? (email ?? "User") : name;
        }
    }
}

using Microsoft.EntityFrameworkCore;

namespace MessagingApp.Models
{
    /// <summary>
    /// Represents a user. Updated for dynamic authentication.
    /// </summary>
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

        // OIDC object id from Entra; used to link external identity to local row
        public string? ExternalObjectId { get; set; }

        public List<Enrollment>? Enrollments { get; set; } // Currently not used

        // Parameterless constructor for EF Core
        public User() { }

        // This one only sets the Name and leaves Email/Password with default empty strings.
        public User(string name)
        {
            Name = name;
        }

        // Full constructor for dynamic authentication and proper seeding.
        public User(string name, string email, string password, string? userType = null)
        {
            Name = name;
            Email = email;
            Password = password;
            UserType = userType;
        }
    }
}

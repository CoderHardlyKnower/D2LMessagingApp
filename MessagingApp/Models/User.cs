namespace MessagingApp.Models
{
    public class User
    {
        /// <summary>
        /// Represents a user. Updated for dynamic authentication.
        /// </summary>
        public int UserId { get; set; }

        // The full name of the user.
        public string Name { get; set; }

        // The user's email, which serves as the login username
        public string Email { get; set; } = string.Empty;

        // For now, no encryption
        public string Password { get; set; } = string.Empty;

        // User type: "student" or "instructor" (potential future usecase)
        public string? UserType { get; set; }

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

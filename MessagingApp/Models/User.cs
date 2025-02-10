namespace MessagingApp.Models
{
    public class User
    {
        /// <summary>
        /// Added a parameterless constructor and made the primary key non-nullable to support EF Core binding
        /// </summary>

        public int UserId { get; set; }
        public string Name { get; set; }
        public string? UserType { get; set; } //User type would be either student or instructor
        public List<Enrollment>? Enrollments { get; set; } //null for now

        public User() { }

        // Convenience constructor for seeding hardcoded data
        public User(string name)
        {
            Name = name;
        }
    }
}
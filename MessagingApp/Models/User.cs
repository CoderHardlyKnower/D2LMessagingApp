namespace MessagingApp.Models
{
    public class User
    {
        public int? UserId { get; set; }
        public string Name { get; set; }
        public string? UserType { get; set; } //User type would be either student or instructor
        public List<Enrollment>? Enrollments { get; set; } //null for now

        public User(string n)
        {
            Name = n;
        }
    }
}

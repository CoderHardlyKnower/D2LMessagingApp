namespace MessagingApp.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public User CourseInstructor { get; set; }
        public List<User> Students { get; set; } = new List<User>();
        public List<Enrollment>? Enrollments { get; set; } // null for now

        // Parameterless constructor
        public Course() { }

        public Course(int id, string n, User ci, List<User> s)
        {
            Id = id;
            Name = n;
            CourseInstructor = ci;
            Students = s;
        }
    }
}

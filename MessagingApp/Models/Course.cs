namespace MessagingApp.Models
{
    public class Course
    {
        public int CourseId { get; set; }
        public string Name { get; set; }

        public int InstructorId { get; set; }
        public User CourseInstructor { get; set; } 

        // Parameterless constructor
        public Course() { }

        public Course(string n, int i_id)
        {
            Name = n;
            InstructorId = i_id;

        }
    }
}

namespace MessagingApp.Models
{
    public class Enrollment
    {
        public int UserId { get; set; }    // Foreign Key to User 
        public User User { get; set; }
        public int CourseId { get; set; }  // Foreign Key to Course 
        public Course Course { get; set;}

        public Enrollment() { }
        public Enrollment(int ui, int ci)
        {
            UserId = ui;
            CourseId = ci;
        }
    }
}

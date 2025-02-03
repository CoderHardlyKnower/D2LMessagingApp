namespace MessagingApp.Models
{
    public class Enrollment
    {
        public int UserId { get; set; }    // Foreign Key to User 
        public int CourseId { get; set; }  // Foreign Key to Course 
    }
}

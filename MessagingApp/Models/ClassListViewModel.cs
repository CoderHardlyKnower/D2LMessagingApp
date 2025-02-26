namespace MessagingApp.Models
{
    public class ClassListViewModel
    {
        public Course Course { get; set; }
        public User Instructor { get; set; }
        public List<User> Students { get; set; }

        public ClassListViewModel() { } 
    }
}

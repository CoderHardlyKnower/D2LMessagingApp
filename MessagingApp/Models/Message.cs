namespace MessagingApp.Models
{
    public class Message
    {
        public int Id { get; set; } // Primary key
        public string Content { get; set; } // Message content
        public DateTime Timestamp { get; set; } = DateTime.Now; // Default value for timestamp
    }
}

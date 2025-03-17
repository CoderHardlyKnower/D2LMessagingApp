namespace MessagingApp.Models
{
    public class Message
    {
        public int Id { get; set; } // Primary key
        public string Content { get; set; } // Message content
        public DateTime Timestamp { get; set; } = DateTime.Now; // Default timestamp
        public bool IsEdited { get; set; } = false; // Indicates if message was edited
        public DateTime CreatedTimestamp { get; set; } = DateTime.Now;



        // Link to the conversation.
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }

        // Sender and Receiver IDs.
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
    }
}

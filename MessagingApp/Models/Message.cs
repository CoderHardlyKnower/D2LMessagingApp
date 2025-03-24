namespace MessagingApp.Models
{
    public class Message
    {
        public int Id { get; set; } // Primary key
        public string Content { get; set; } // Message content
        public DateTime Timestamp { get; set; } = DateTime.Now; // Default timestamp that is used for editing
        public bool IsEdited { get; set; } = false; // Indicates if message was edited
        public DateTime CreatedTimestamp { get; set; } = DateTime.Now; // Timestamp only for NEW messages
        public bool IsRead { get; set; } = false;  // Default: Unread
        public bool IsDeleted { get; set; } = false; // Default: Not Deleted
        public bool IsTyping { get; set; } = false; // Future Typing Indicator


        // Link to the conversation.
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }

        // Sender and Receiver IDs.
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
    }
}

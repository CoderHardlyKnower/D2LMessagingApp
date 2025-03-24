namespace MessagingApp.Models
{
    public class ConversationParticipant
    {
        // Composite key: ConversationId + UserId
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        // Tracks the last time the user viewed a convo
        public DateTime LastRead { get; set; } = DateTime.Now;
    }
}

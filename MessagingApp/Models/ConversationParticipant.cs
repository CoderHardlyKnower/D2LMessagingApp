namespace MessagingApp.Models
{
    public class ConversationParticipant
    {
        // Composite key: ConvesrationId + UserId
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}

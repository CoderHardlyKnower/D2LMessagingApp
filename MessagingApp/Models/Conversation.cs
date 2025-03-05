using System.ComponentModel.DataAnnotations;

namespace MessagingApp.Models
{
    public class Conversation
    {
        [Key]
        public int ConversationId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}

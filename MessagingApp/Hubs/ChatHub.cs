using Microsoft.AspNetCore.SignalR;
using MessagingApp.Data;
using MessagingApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MessagingApp.Hubs
{
    /// <summary>
    /// SignalR hub for handling real-time messaging operations.
    /// Provides methods for sending, editing, deleting messages, and managing conversation groups.
    /// </summary>
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(int senderId, string senderName, string message, int conversationId)
        {
            var newMessage = new Message
            {
                SenderId = senderId,
                ReceiverId = 0,
                Content = message,
                CreatedTimestamp = DateTime.Now,
                Timestamp = DateTime.Now,
                ConversationId = conversationId,
                IsRead = false // Ensure message is unread by default
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            // Broadcast new message
            await Clients.Group("conversation_" + conversationId)
                .SendAsync("ReceiveMessage", senderId, senderName, message, newMessage.Timestamp.ToShortTimeString(), newMessage.Id);

            // Notify unread messages for real-time UI updates
            await Clients.All.SendAsync("UpdateConversations");
        }

        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "conversation_" + conversationId);
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "conversation_" + conversationId);
        }

        // Mark messages as read
        public async Task MarkMessagesAsRead(int userId, int conversationId)
        {
            // Mark all unread messages from the other user as read.
            var messages = _context.Messages
                .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead)
                .ToList();

            if (messages.Any())
            {
                messages.ForEach(m => m.IsRead = true);
            }

            // Update the participant's LastRead so that GetRecentConversations shows no unread messages.
            var participant = _context.ConversationParticipants
                .FirstOrDefault(p => p.ConversationId == conversationId && p.UserId == userId);

            if (participant != null)
            {
                participant.LastRead = DateTime.Now;
            }

            await _context.SaveChangesAsync();

                // Notify UI updates
            await Clients.All.SendAsync("UpdateConversations");
        }

        // Handle typing indicators
        public async Task TypingIndicator(int conversationId, int userId, bool isTyping)
        {
            await Clients.Group("conversation_" + conversationId)
                .SendAsync("UserTyping", userId, isTyping);
        }

        public async Task BroadcastUpdateConversations()
        {
            await Clients.All.SendAsync("UpdateConversations");
        }

        // Edit message (with IsEdited flag)
        public async Task EditMessage(int messageId, string newContent)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message != null)
            {
                message.Content = newContent;
                message.Timestamp = DateTime.Now;
                message.IsEdited = true;
                await _context.SaveChangesAsync();

                await Clients.All.SendAsync("MessageEdited", messageId, newContent, message.Timestamp.ToShortTimeString());
            }
        }

        // Delete message
        public async Task DeleteMessage(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message != null)
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();

                await Clients.All.SendAsync("MessageDeleted", messageId);
            }
        }
    }
}

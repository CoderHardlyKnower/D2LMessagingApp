using Microsoft.AspNetCore.SignalR;
using MessagingApp.Data;
using MessagingApp.Models;
using System;
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
            // Save the message to the database
            var newMessage = new Message
            {
                SenderId = senderId,
                ReceiverId = 0,
                Content = message,
                Timestamp = DateTime.Now,
                ConversationId = conversationId
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            // Send the newMessage.Id along with other data so the client can attach it
            await Clients.Group("conversation_" + conversationId)
                .SendAsync("ReceiveMessage", senderId, senderName, message, newMessage.Timestamp.ToShortTimeString(), newMessage.Id);
        }

        public async Task JoinConversation(int conversationId)
        {
            //Clents in a conversation will be added to a group named "conversation_{conversationId}"
            await Groups.AddToGroupAsync(Context.ConnectionId, "conversation_" + conversationId);
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "conversation_" + conversationId);
        }


        // Edit a message
        public async Task EditMessage(int messageId, string newContent)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message != null)
            {
                message.Content = newContent;
                message.Timestamp = DateTime.Now;
                message.IsEdited = true; // Mark the message as edited
                await _context.SaveChangesAsync();

                await Clients.All.SendAsync("MessageEdited", messageId, newContent, message.Timestamp.ToShortTimeString());
            }
        }


        // Delete a message
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

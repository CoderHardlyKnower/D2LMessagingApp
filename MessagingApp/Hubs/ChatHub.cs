using Microsoft.AspNetCore.SignalR;
using MessagingApp.Data;
using MessagingApp.Models;
using System;
using System.Threading.Tasks;

namespace MessagingApp.Hubs
{
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

            
            await Clients.All.SendAsync("ReceiveMessage", senderId, senderName, message, newMessage.Timestamp.ToShortTimeString());
        }
    }
}

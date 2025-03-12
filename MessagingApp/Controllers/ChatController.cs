using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MessagingApp.Data;
using MessagingApp.Models;
using System.Security.Claims;

namespace MessagingApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        //Get recent conversations for chat window
        public async Task<IActionResult> GetRecentConversations()
        {
            //Get logged in user
            int loggedInUserId = int.Parse(User.FindFirst("UserId").Value);

            // Get recent conversations 
            var conversations = await _context.Conversations
                .Where(c => c.Participants.Any(p => p.UserId == loggedInUserId) && c.Messages.Any())
                .Select(c => new
                {
                    c.ConversationId,
                    LastMessage = c.Messages.OrderByDescending(m => m.Timestamp).FirstOrDefault().Content,

                    MessageSender = c.Participants
                        .Where(p => p.UserId != loggedInUserId)
                        .Select(p => p.User.Name)
                        .FirstOrDefault(),

                    LastMessageTimestamp = c.Messages.OrderByDescending(m => m.Timestamp).FirstOrDefault().Timestamp
                })
                .OrderByDescending(c => c.LastMessageTimestamp)
                .Take(6) // Limit to 6 conversations for now
                .ToListAsync();


            return Json(conversations);
        }
    }
}
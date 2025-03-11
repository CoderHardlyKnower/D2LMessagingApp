using Microsoft.AspNetCore.Mvc;
using MessagingApp.Data;
using MessagingApp.Models;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace MessagingApp.Controllers
{
    /// <summary>
    /// MessagingController handles the display and sending of messages for distinct conversations.
    /// It now uses the Conversations table to group messages between two users.
    /// </summary>
    [Authorize]
    public class MessagingController : Controller
    {
        private readonly AppDbContext _context;

        public MessagingController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays the messaging page for the selected student.
        /// Retrieves the conversation between the logged-in user and the selected student
        /// loads messages for that conversation ordered by timestamp.
        /// </summary>
        /// <param name="studentId">The ID of the selected student</param>
        /// <param name="studentName">The full name of the selected student</param>
        /// <returns>The messaging view with messages for the conversation</returns>
        public async Task<IActionResult> Index(int studentId, string studentName)
        {
            // Get logged-in user's ID from claims.
            int loggedInUserId = int.Parse(User.FindFirst("UserId").Value);

            // Get or create a conversation between the logged-in user and the selected student
            var conversation = await GetOrCreateConversationAsync(loggedInUserId, studentId);

            // Retrieve messages for this conversation, ordered by timestamp.
            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversation.ConversationId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            // Build a dictionary mapping sender IDs to names for display.
            var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
            var userNames = await _context.Users
                .Where(u => senderIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u.Name);

            ViewBag.UserNames = userNames;
            ViewBag.StudentName = studentName;
            ViewBag.StudentId = studentId;
            ViewBag.ConversationId = conversation.ConversationId;
            return View(messages);
        }

        /// <summary>
        /// Adds a new message to the current conversation and refreshes the messaging view.
        /// </summary>
        /// <param name="studentId">The ID of the selected student (chat partner)</param>
        /// <param name="content">The message content</param>
        /// <param name="studentName">The full name of the selected student</param>
        /// <param name="conversationId">The ConversationId of the current chat</param>
        /// <returns>Redirects to the messaging view for the conversation</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMessage(int studentId, string content, string studentName, int conversationId)
        {
            if (!string.IsNullOrEmpty(content))
            {
                int loggedInUserId = int.Parse(User.FindFirst("UserId").Value);
                var message = new Message
                {
                    Content = content,
                    Timestamp = DateTime.Now,
                    SenderId = loggedInUserId,
                    ReceiverId = studentId,
                    ConversationId = conversationId
                };
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", new { studentId, studentName });
        }

        /// <summary>
        /// Retrieves an existing conversation between two users or creates a new one if none exists.
        /// This method ensures that each one-to-one chat has a unique ConversationId.
        /// </summary>
        /// <param name="userA">The logged-in user's ID</param>
        /// <param name="userB">The selected student's user ID</param>
        /// <returns>A Conversation object representing the private chat</returns>
        private async Task<Conversation> GetOrCreateConversationAsync(int userA, int userB)
        {
            // Look for an existing conversation that has exactly these two participants.
            var conversation = await _context.Conversations
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c =>
                    c.Participants.Count == 2 &&
                    c.Participants.Any(p => p.UserId == userA) &&
                    c.Participants.Any(p => p.UserId == userB));

            if (conversation == null)
            {
                // Create a new conversation and save to generate a ConversationId.
                conversation = new Conversation();
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();

                // Add both users as participants.
                var participantA = new ConversationParticipant { ConversationId = conversation.ConversationId, UserId = userA };
                var participantB = new ConversationParticipant { ConversationId = conversation.ConversationId, UserId = userB };
                _context.ConversationParticipants.AddRange(participantA, participantB);
                await _context.SaveChangesAsync();
            }
            return conversation;
        }
    }
}

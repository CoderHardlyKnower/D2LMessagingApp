using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MessagingApp.Data;
using MessagingApp.Hubs;
using MessagingApp.Models;
using MessagingApp.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MessagingApp.Controllers
{
    /// <summary>
    /// MessagingController handles the display and sending of messages for distinct conversations.
    /// It now groups messages and pushes them in real time via SignalR.
    /// </summary>
    [Authorize]
    public class MessagingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorage;
        private readonly IHubContext<ChatHub> _hub;

        public MessagingController(
            AppDbContext context,
            IFileStorageService fileStorage,
            IHubContext<ChatHub> hub // injected for real-time pushes
        )
        {
            _context = context;
            _fileStorage = fileStorage;
            _hub = hub;
        }

        /// <summary>
        /// Displays the messaging page for the selected student.
        /// </summary>
        public async Task<IActionResult> Index(int studentId, string studentName)
        {
            int loggedInUserId = int.Parse(User.FindFirst("UserId").Value);
            var conversation = await GetOrCreateConversationAsync(loggedInUserId, studentId);

            var participant = conversation.Participants.FirstOrDefault(p => p.UserId == loggedInUserId);
            if (participant != null)
            {
                participant.LastRead = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversation.ConversationId)
                .OrderBy(m => m.CreatedTimestamp)
                .ToListAsync();

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
        /// Handles form POST for a new message and optional file attachment.
        /// Saves to DB, uploads file locally, then broadcasts via SignalR.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMessage(
     int studentId,
     string content,
     string studentName,
     int conversationId,
     IFormFile? attachment)
        {
            // Allow: text OR file OR both
            if (string.IsNullOrWhiteSpace(content) && (attachment == null || attachment.Length == 0))
            {
                return BadRequest("No content or attachment provided.");
            }

            // --- Server-side guardrails ---
            const long MaxUploadBytes = 25L * 1024 * 1024; // 25 MB
            var allowedExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".pdf" };

            string? attachmentUrl = null;

            if (attachment != null && attachment.Length > 0)
            {
                if (attachment.Length > MaxUploadBytes)
                    return BadRequest($"File too large. Max {MaxUploadBytes / (1024 * 1024)} MB.");

                var ext = Path.GetExtension(attachment.FileName);
                if (!allowedExt.Contains(ext))
                    return BadRequest("Unsupported file type. Allowed: images (png/jpg/jpeg/gif/webp) and PDF.");

                // Upload to Blob Storage (IFileStorageService)
                using var stream = attachment.OpenReadStream();
                attachmentUrl = await _fileStorage.UploadAsync(stream, attachment.FileName);
            }

            int loggedInUserId = int.Parse(User.FindFirst("UserId")!.Value);

            var message = new Message
            {
                Content = content ?? string.Empty,
                Timestamp = DateTime.Now,
                CreatedTimestamp = DateTime.Now,
                SenderId = loggedInUserId,
                ReceiverId = studentId,
                ConversationId = conversationId,
                IsRead = false,
                // requires public string? AttachmentUrl { get; set; } in your model
                AttachmentUrl = attachmentUrl
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Push to clients in this conversation (your client already listens for ReceiveMessage)
            await _hub.Clients.Group($"conversation_{conversationId}")
                .SendAsync("ReceiveMessage",
                    message.SenderId,
                    User.Identity!.Name,
                    message.Content,
                    message.Timestamp.ToShortTimeString(),
                    message.Id,
                    message.AttachmentUrl);

            await _hub.Clients.All.SendAsync("UpdateConversations");

            return Ok(new { messageId = message.Id, attachment = message.AttachmentUrl != null });
        }


        /// <summary>
        /// Ensures a two-user conversation exists, creating if necessary.
        /// </summary>
        private async Task<Conversation> GetOrCreateConversationAsync(int userA, int userB)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c =>
                    c.Participants.Count == 2 &&
                    c.Participants.Any(p => p.UserId == userA) &&
                    c.Participants.Any(p => p.UserId == userB)
                );

            if (conversation == null)
            {
                conversation = new Conversation();
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();

                var pa = new ConversationParticipant { ConversationId = conversation.ConversationId, UserId = userA, LastRead = DateTime.Now };
                var pb = new ConversationParticipant { ConversationId = conversation.ConversationId, UserId = userB, LastRead = DateTime.Now };
                _context.ConversationParticipants.AddRange(pa, pb);
                await _context.SaveChangesAsync();
            }
            return conversation;
        }

        /// <summary>
        /// Returns recent conversations for the current user.
        /// </summary>
        //Get recent conversations for chat window in course selection
        public async Task<IActionResult> GetRecentConversations(int excludeConversationId = 0)
        {
            int loggedInUserId = int.Parse(User.FindFirst("UserId").Value);
            var conversations = await (
                from c in _context.Conversations
                where c.Participants.Any(p => p.UserId == loggedInUserId) && c.Messages.Any()
                where excludeConversationId == 0 || c.ConversationId != excludeConversationId
                let lastMsg = c.Messages.OrderByDescending(m => m.Timestamp).FirstOrDefault()
                select new
                {
                    c.ConversationId,
                    LastMessage = lastMsg.Content,                 
                    LastMessageTimestamp = lastMsg.Timestamp,
                    LastMessageSenderId = lastMsg.SenderId,

                    // New: attachment flags for UI
                    HasAttachment = !string.IsNullOrEmpty(lastMsg.AttachmentUrl),
                    IsFileOnly = !string.IsNullOrEmpty(lastMsg.AttachmentUrl) && string.IsNullOrWhiteSpace(lastMsg.Content),

                    missedCount = c.Messages.Count(m =>
                        m.Timestamp > c.Participants.FirstOrDefault(p => p.UserId == loggedInUserId).LastRead
                        && m.SenderId != loggedInUserId),

                    Student = c.Participants
                                .Where(p => p.UserId != loggedInUserId)
                                .Select(p => new { p.User.UserId, p.User.Name })
                                .FirstOrDefault()
                }
            )
            .OrderByDescending(c => c.LastMessageTimestamp)
            .ToListAsync();

            return Json(conversations);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using MessagingApp.Data;
using MessagingApp.Models;
using System;
using System.Linq;

namespace MessagingApp.Controllers
{
    /// <summary>
    /// MessagingController handles the display and sending of messages
    /// Currently, it loads all messages that have been sent and displays the selected students name
    /// We may want to add conversation filtering between selected students
    /// </summary>

    public class MessagingController : Controller
    {
        private readonly AppDbContext _context;

        public MessagingController(AppDbContext context)
        {
            _context = context;
        }

        // Display messaging page for the selected student
        public IActionResult Index(int studentId, string studentName)
        {
            // Get the logged-in user's ID from claims.
            int loggedInUserId = int.Parse(User.FindFirst("UserId").Value);

            // Retrieve messages only between the logged-in user and the selected student.
            var messages = _context.Messages
                .Where(m => (m.SenderId == loggedInUserId && m.ReceiverId == studentId) ||
                            (m.SenderId == studentId && m.ReceiverId == loggedInUserId))
                .OrderBy(m => m.Timestamp)
                .ToList();

            // Build a dictionary mapping sender IDs to names.
            // Get all distinct sender IDs from the messages.
            var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
            // Query the Users table for those sender IDs.
            var userNames = _context.Users
                .Where(u => senderIds.Contains(u.UserId))
                .ToDictionary(u => u.UserId, u => u.Name);

            ViewBag.UserNames = userNames;
            ViewBag.StudentName = studentName;
            ViewBag.StudentId = studentId;
            return View(messages);
        }

        // Add a new message and then refresh the messaging view
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMessage(int studentId, string content, string studentName)
        {
            if (!string.IsNullOrEmpty(content))
            {
                int loggedInUserId = int.Parse(User.FindFirst("UserId").Value);
                var message = new Message
                {
                    Content = content,
                    Timestamp = DateTime.Now,
                    SenderId = loggedInUserId,
                    ReceiverId = studentId
                };
                _context.Messages.Add(message);
                _context.SaveChanges();
            }
            return RedirectToAction("Index", new { studentId, studentName });
        }
    }
    
}

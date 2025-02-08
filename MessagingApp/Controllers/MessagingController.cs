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
            // For now, we display all messages; later, filter by conversation.
            var messages = _context.Messages.ToList();
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
                var message = new Message { Content = content, Timestamp = DateTime.Now };
                _context.Messages.Add(message);
                _context.SaveChanges();
            }
            return RedirectToAction("Index", new { studentId, studentName });
        }
    }
}

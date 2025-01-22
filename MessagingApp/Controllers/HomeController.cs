using Microsoft.AspNetCore.Mvc;
using MessagingApp.Data;
using MessagingApp.Models;
using System.Linq;

namespace MessagingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // Display messages
        public IActionResult Index()
        {
            var messages = _context.Messages.ToList();
            return View(messages);
        }

        // Add a new message
        [HttpPost]
        public IActionResult AddMessage(string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                var message = new Message { Content = content };
                _context.Messages.Add(message);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}

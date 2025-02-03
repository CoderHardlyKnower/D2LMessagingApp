using Microsoft.AspNetCore.Mvc;
using MessagingApp.Data;
using MessagingApp.Models;
using System.Linq;
using MessagingApp.Migrations;

namespace MessagingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        List<User> users = new List<User>();
        List<Course> courses = new List<Course>();

        public HomeController(AppDbContext context)
        {
            _context = context;

            //hardcoded data
            User student1 = new User("John");
            User student2 = new User("Alice");
            User instructor = new User("Bob");
            users.AddRange(new List<User> { student1, student2, instructor});

            Course course1 = new Course(1, "Web programming", instructor, new List<User> {student1, student2 });
            Course course2 = new Course(2, "C#", instructor, new List<User> {student2});
            courses.AddRange(new List<Course> { course1, course2 });
        }

        // Display messages
        public IActionResult Index()
        {
            var messages = _context.Messages.ToList();
            return View(messages);
        }

        //Display landing page
        public IActionResult LandingPage()
        {
            return View(courses);  
        }

        //Display class list
        public IActionResult ClassList(int id)
        {
            var course = courses.FirstOrDefault(x => x.Id == id);
            
            return View(course);
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

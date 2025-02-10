using Microsoft.AspNetCore.Mvc;
using MessagingApp.Data;
using MessagingApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace MessagingApp.Controllers
{
    public class CoursesController : Controller
    {
        /// <summary>
        /// CoursesController manages the course selection and class list pages
        /// We are currently using hardcoded data for demonstration purposes for iteration 1
        /// </summary>

        private readonly AppDbContext _context;

        private List<User> users = new List<User>();
        private List<Course> courses = new List<Course>();

        public CoursesController(AppDbContext context)
        {
            _context = context;

            // Hardcoded data with explicit IDs and UserType assignments for routing and clarity
            User student1 = new User("John") { UserId = 1, UserType = "student" };
            User student2 = new User("Alice") { UserId = 2, UserType = "student" };
            User instructor = new User("Bob") { UserId = 3, UserType = "instructor" };
            users.AddRange(new List<User> { student1, student2, instructor });

            // Hardcoded courses
            Course course1 = new Course(1, "Web programming", instructor, new List<User> { student1, student2 });
            Course course2 = new Course(2, "C#", instructor, new List<User> { student2 });
            courses.AddRange(new List<Course> { course1, course2 });
        }

        // Landing page: display list of courses (course selection)
        public IActionResult LandingPage()
        {
            return View("CourseSelection", courses);
        }

        // Class list: display details (instructor and students) for a selected course
        public IActionResult ClassList(int id)
        {
            var course = courses.FirstOrDefault(x => x.Id == id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }
    }
}

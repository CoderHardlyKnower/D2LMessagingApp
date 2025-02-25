using Microsoft.AspNetCore.Mvc;
using MessagingApp.Data;
using MessagingApp.Models;


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

            // Create our objects corresponding to seeded accounts.
            var userAustin = new User("Austin Brown") { UserId = 1, UserType = "student", Email = "Abrown9034@conestogac.on.ca" };
            var userKhemara = new User("Khemara Oeun") { UserId = 2, UserType = "student", Email = "Koeun8402@conestogac.on.ca" };
            var userAmanda = new User("Amanda Esteves") { UserId = 3, UserType = "student", Email = "Aesteves3831@conestogac.on.ca" };
            var userTristan = new User("Tristan Lagace") { UserId = 4, UserType = "student", Email = "Tlagace9030@conestogac.on.ca" };

            // For instructor, we'll use a hardcoded user or another seeded user.
            var instructor = new User("Bob") { UserId = 5, UserType = "instructor", Email = "bob@conestogac.on.ca" };

            // Create courses – each course includes all four of us as students.
            var course1 = new Course(1, "Web Programming", instructor, new List<User> { userAustin, userKhemara, userAmanda, userTristan });
            var course2 = new Course(2, "C#", instructor, new List<User> { userAustin, userKhemara, userAmanda, userTristan });
            courses.Add(course1);
            courses.Add(course2);
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

            // Remove logged-in user from the student list using the Email claim.
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUserEmail = User.FindFirst("Email")?.Value;
                if (!string.IsNullOrEmpty(currentUserEmail))
                {
                    course.Students = course.Students.Where(s => s.Email != currentUserEmail).ToList();
                }
            }
            return View(course);
        }
    }
}

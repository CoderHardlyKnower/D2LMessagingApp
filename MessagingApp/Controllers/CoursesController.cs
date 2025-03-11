using Microsoft.AspNetCore.Mvc;
using MessagingApp.Data;
using MessagingApp.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace MessagingApp.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        /// <summary>
        /// CoursesController manages course selection and class lists.
        /// </summary>
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        // Landing page: display list of courses the logged in student is enrolled in.
        public async Task<IActionResult> LandingPage()
        {
            int userId = GetStudentId();
            var courses = await GetStudentCourses(userId);
            return View("CourseSelection", courses);
        }

        // Class list: display details (instructor and students) for a selected course.
        // Excludes the logged-in user from the student list.
        public async Task<IActionResult> ClassList(int id)
        {
            // Fetch the course along with its instructor.
            var course = await _context.Courses
                .Include(c => c.CourseInstructor)
                .FirstOrDefaultAsync(x => x.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            // Fetch all enrolled students.
            var allStudents = await GetEnrolledStudents(course.CourseId);

            // Exclude the logged-in student.
            int userId = GetStudentId();
            var students = allStudents.Where(s => s.UserId != userId).ToList();

            var viewModel = new ClassListViewModel
            {
                Course = course,
                Instructor = course.CourseInstructor,
                Students = students
            };

            return View(viewModel);
        }

        // Retrieve the courses the logged-in student is enrolled in.
        public async Task<List<Course>> GetStudentCourses(int userId)
        {
            var courses = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.Course)
                .ToListAsync();
            return courses;
        }

        // Retrieve the list of students enrolled in a given course.
        public async Task<List<User>> GetEnrolledStudents(int courseId)
        {
            var students = await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Select(e => e.User)
                .ToListAsync();
            return students;
        }

        // Retrieve logged in student's ID from claims.
        int GetStudentId()
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            return int.Parse(userIdString);
        }
    }
}

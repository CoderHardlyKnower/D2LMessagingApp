using Microsoft.AspNetCore.Mvc;
using MessagingApp.Data;
using MessagingApp.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MessagingApp.Controllers
{
    public class CoursesController : Controller
    {
        /// <summary>
        /// CoursesController manages the course selection and class list pages
        /// </summary>

        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        // Landing page: display list of courses (course selection)
        public async Task<IActionResult> LandingPage()
        {
            int userId = GetStudentId();
            var courses = await GetStudentCourses(userId);

            return View("CourseSelection", courses);
        }

        // Class list: display details (instructor and students) for a selected course
        public async Task<IActionResult> ClassList(int id)
        {
            // Fetch the course 
            var course = await _context.Courses
                .Include(c => c.CourseInstructor)
                .FirstOrDefaultAsync(x => x.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            //Fetch all students enrolled in course
            var allStudents = await GetEnrolledStudents(course.CourseId);

            //Get list of students excluding student that is logged in
            int userId = GetStudentId();
            var students = allStudents.Where(s => s.UserId != userId).ToList();


            //Creating model object to pass to the view
            var viewModel = new ClassListViewModel
            {
                Course = course,
                Instructor = course.CourseInstructor, 
                Students = students
            };

            return View(viewModel);
        }

        //Method for fetching courses student is enrolled in
        public async Task<List<Course>> GetStudentCourses(int userId)
        {
            var courses = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.Course)
                .ToListAsync();

            return courses;
        }

        //Method for fetching stundets enrolled in a course
        public async Task<List<User>> GetEnrolledStudents(int courseId)
        {
            var students = await _context.Enrollments
            .Where(e => e.CourseId == courseId)
            .Select(e => e.User)  
            .ToListAsync();

            return students;
        }


        //Get logged in student id
        int GetStudentId()
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            int userId = int.Parse(userIdString);
            return userId;
        }
    }
}

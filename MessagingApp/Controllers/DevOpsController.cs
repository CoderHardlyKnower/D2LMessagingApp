using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessagingApp.Data;

[AllowAnonymous] // dev-only; remove or guard before shipping
public class DevOpsController : Controller
{
    private readonly AppDbContext _db;
    public DevOpsController(AppDbContext db) => _db = db;

    // GET /DevOps/SeedAndStatus
    [HttpGet]
    public IActionResult SeedAndStatus()
    {
        _db.Database.Migrate();
        SeedCoursesIfEmpty(_db); 

        var courseCount = _db.Courses.Count();
        var userCount = _db.Users.Count();
        var enrCount = _db.Enrollments.Count();

        return Content($"Courses={courseCount}, Users={userCount}, Enrollments={enrCount}");
    }

    // GET /DevOps/EnrollCurrentUserInAllCourses 
    [HttpGet]
    public async Task<IActionResult> EnrollCurrentUserInAllCourses()
    {
        var userIdStr = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrWhiteSpace(userIdStr)) return Content("Not signed in.");
        var userId = int.Parse(userIdStr);

        var allCourseIds = await _db.Courses.Select(c => c.CourseId).ToListAsync();
        var already = await _db.Enrollments
                                   .Where(e => e.UserId == userId)
                                   .Select(e => e.CourseId)
                                   .ToListAsync();
        var toAdd = allCourseIds.Except(already).ToList();

        foreach (var cid in toAdd)
            _db.Enrollments.Add(new MessagingApp.Models.Enrollment(userId, cid));

        if (toAdd.Count > 0) await _db.SaveChangesAsync();
        return Content($"Enrolled user {userId} into {toAdd.Count} course(s).");
    }

    // call existing seeder from here
    private static void SeedCoursesIfEmpty(AppDbContext db)
    {
        if (!db.Courses.Any())
        {
            db.Courses.AddRange(
                new MessagingApp.Models.Course("Web Programming", 0),
                new MessagingApp.Models.Course("C# Programming", 0),
                new MessagingApp.Models.Course("Mobile Development", 0),
                new MessagingApp.Models.Course("User Experience", 0),
                new MessagingApp.Models.Course("Programming Concepts II", 0),
                new MessagingApp.Models.Course("Database:SQL", 0)
            );
            db.SaveChanges();
        }
    }
}

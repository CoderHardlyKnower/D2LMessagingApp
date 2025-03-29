using Microsoft.EntityFrameworkCore;
using System;
using MessagingApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using MessagingApp.Models;
using MessagingApp.Controllers;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();


// Configure SQL Server 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    //context.database.migrate();

    SeedDatabase(context);
}

app.MapHub<MessagingApp.Hubs.ChatHub>("/chatHub");

app.Run();

//Seed database method
void SeedDatabase(AppDbContext context)
{
    // Seed Users if none exist.
    if (!context.Users.Any())
    {
        context.Users.AddRange(new List<User>
        {
            new User("Austin Brown", "Abrown9034@conestogac.on.ca", "password1", "student"),
            new User("Khemara Oeun", "Koeun8402@conestogac.on.ca", "password2", "student"),
            new User("Amanda Esteves", "Aesteves3831@conestogac.on.ca", "password3", "student"),
            new User("Tristan Lagace", "Tlagace9030@conestogac.on.ca", "password4", "student"),
            new User("Isabella Ramirez", "iramirez@conestogac.on.ca", "password5", "student"),
            new User("Mohammed Al-Farouq", "maalfarouq@conestogac.on.ca", "password6", "student"),
            new User("Sienna Nguyen", "snguyen@conestogac.on.ca", "password7", "student"),
            new User("Diego Morales", "dmorales@conestogac.on.ca", "password8", "student")
        });
        context.SaveChanges();
    }

    //Add instructor if none exists
    var instructor = context.Users.FirstOrDefault(u => u.UserType == "instructor");
    if (instructor == null)
    {
        instructor = new User("Caroline Mercer", "c.mercer@conestogac.on.ca", "password5", "instructor");
        context.Users.Add(instructor);
        context.SaveChanges();
    }

    //Seed courses if none exists
    if (!context.Courses.Any())
    {
        int instructorId = context.Users.FirstOrDefault(u => u.UserType == "instructor").UserId;
        context.Courses.AddRange(new List<Course>
            {
                new Course("Web Programming", instructorId),
                new Course("C# Programming", instructorId),
                new Course("Mobile Development", instructorId),
                new Course("User Experience", instructorId),
                new Course("Programming Concepts II", instructorId),
                new Course("Database:SQL", instructorId)
            });

        context.SaveChanges();
    }

    //seed enrollments
    if (!context.Enrollments.Any())
    {
        if (context.Users.Any() && context.Courses.Any())
        {
            var students = context.Users.Where(u => u.UserType == "student").ToList();
            var courses = context.Courses.ToList();

            context.Enrollments.AddRange(new List<Enrollment>
                {
                    // Austin, Khemara, Amanda
                    new Enrollment(students[0].UserId, courses[0].CourseId), // Web Programming
                    new Enrollment(students[0].UserId, courses[2].CourseId), // Mobile Development
                    new Enrollment(students[0].UserId, courses[3].CourseId), // User Experience
                    new Enrollment(students[0].UserId, courses[4].CourseId), // Programming Concepts II

                    new Enrollment(students[1].UserId, courses[0].CourseId),
                    new Enrollment(students[1].UserId, courses[2].CourseId),
                    new Enrollment(students[1].UserId, courses[3].CourseId),
                    new Enrollment(students[1].UserId, courses[4].CourseId),

                    new Enrollment(students[2].UserId, courses[0].CourseId),
                    new Enrollment(students[2].UserId, courses[2].CourseId),
                    new Enrollment(students[2].UserId, courses[3].CourseId),
                    new Enrollment(students[2].UserId, courses[4].CourseId),

                    // Tristan: use Database:SQL instead of Programming Concepts II
                    new Enrollment(students[3].UserId, courses[0].CourseId),
                    new Enrollment(students[3].UserId, courses[2].CourseId),
                    new Enrollment(students[3].UserId, courses[3].CourseId),
                    new Enrollment(students[3].UserId, courses[5].CourseId),

                    // New students:
                    new Enrollment(students[4].UserId, courses[0].CourseId),
                    new Enrollment(students[4].UserId, courses[1].CourseId),
                    new Enrollment(students[4].UserId, courses[2].CourseId),
                    new Enrollment(students[4].UserId, courses[3].CourseId),
                    new Enrollment(students[4].UserId, courses[5].CourseId),

                    new Enrollment(students[5].UserId, courses[0].CourseId),
                    new Enrollment(students[5].UserId, courses[1].CourseId),
                    new Enrollment(students[5].UserId, courses[2].CourseId),
                    new Enrollment(students[5].UserId, courses[3].CourseId),
                    new Enrollment(students[5].UserId, courses[5].CourseId),

                    new Enrollment(students[6].UserId, courses[0].CourseId),
                    new Enrollment(students[6].UserId, courses[1].CourseId),
                    new Enrollment(students[6].UserId, courses[2].CourseId),
                    new Enrollment(students[6].UserId, courses[3].CourseId),
                    new Enrollment(students[6].UserId, courses[5].CourseId),

                    new Enrollment(students[7].UserId, courses[0].CourseId),
                    new Enrollment(students[7].UserId, courses[1].CourseId),
                    new Enrollment(students[7].UserId, courses[2].CourseId),
                    new Enrollment(students[7].UserId, courses[3].CourseId),
                    new Enrollment(students[7].UserId, courses[5].CourseId),

                });

            context.SaveChanges();
        }
    }
}

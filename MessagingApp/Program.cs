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

    context.Database.Migrate();

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
            new User("Tristan Lagace", "Tlagace9030@conestogac.on.ca", "password4", "student")
        });
        context.SaveChanges();
    }

    //Add instructor if none exists
    var instructor = context.Users.FirstOrDefault(u => u.UserType == "instructor");
    if (instructor == null)
    {
        instructor = new User("John Smith", "john.smith@conestogac.on.ca", "password5", "instructor" );
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
                new Course("Mobile Development", instructorId)
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
                    //Web programming
                    new Enrollment(students[0].UserId, courses[0].CourseId),
                    new Enrollment(students[1].UserId, courses[0].CourseId),

                    //C#
                    new Enrollment(students[2].UserId, courses[1].CourseId),
                    new Enrollment(students[3].UserId, courses[1].CourseId),

                    //Mobile development
                    new Enrollment(students[0].UserId, courses[2].CourseId),
                    new Enrollment(students[1].UserId, courses[2].CourseId),
                    new Enrollment(students[2].UserId, courses[2].CourseId),
                    new Enrollment(students[3].UserId, courses[2].CourseId),

                });

                context.SaveChanges();
            }
    }
}
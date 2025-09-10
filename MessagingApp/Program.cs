using Microsoft.EntityFrameworkCore;
using System;
using MessagingApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using MessagingApp.Models;
using MessagingApp.Controllers;
using Azure.Storage.Blobs;
using MessagingApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// File storage service
//builder.Services.AddSingleton(sp =>
//    new BlobServiceClient(
//        builder.Configuration["Azure:StorageConnectionString"]
//    )
//);
//builder.Services.AddTransient<IFileStorageService, AzureBlobStorageService>();
builder.Services.AddTransient<IFileStorageService, LocalFileStorageService>();

// Configure SQL Server 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

// Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// SignalR with fallback for local dev
var asrs = builder.Configuration.GetConnectionString("AzureSignalR")
          ?? builder.Configuration["Azure:SignalR:ConnectionString"];

if (!string.IsNullOrWhiteSpace(asrs) && !builder.Environment.IsDevelopment())
{
    builder.Services.AddSignalR().AddAzureSignalR(asrs);
}
else
{
    builder.Services.AddSignalR();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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

// Map the SignalR hub
app.MapHub<MessagingApp.Hubs.ChatHub>("/chathub");

app.Run();


// ---- Seed method (unchanged) ----
void SeedDatabase(AppDbContext context)
{
    if (!context.Users.Any())
    {
        context.Users.AddRange(new List<MessagingApp.Models.User>
        {
            new MessagingApp.Models.User("Austin Brown", "Abrown9034@conestogac.on.ca", "password1", "student"),
            new MessagingApp.Models.User("Khemara Oeun", "Koeun8402@conestogac.on.ca", "password2", "student"),
            new MessagingApp.Models.User("Amanda Esteves", "Aesteves3831@conestogac.on.ca", "password3", "student"),
            new MessagingApp.Models.User("Tristan Lagace", "Tlagace9030@conestogac.on.ca", "password4", "student"),
            new MessagingApp.Models.User("Isabella Ramirez", "iramirez@conestogac.on.ca", "password5", "student"),
            new MessagingApp.Models.User("Mohammed Al-Farouq", "maalfarouq@conestogac.on.ca", "password6", "student"),
            new MessagingApp.Models.User("Sienna Nguyen", "snguyen@conestogac.on.ca", "password7", "student"),
            new MessagingApp.Models.User("Diego Morales", "dmorales@conestogac.on.ca", "password8", "student")
        });
        context.SaveChanges();
    }

    var instructor = context.Users.FirstOrDefault(u => u.UserType == "instructor");
    if (instructor == null)
    {
        instructor = new MessagingApp.Models.User("Caroline Mercer", "c.mercer@conestogac.on.ca", "password5", "instructor");
        context.Users.Add(instructor);
        context.SaveChanges();
    }

    if (!context.Courses.Any())
    {
        int instructorId = context.Users.First(u => u.UserType == "instructor").UserId;
        context.Courses.AddRange(new List<MessagingApp.Models.Course>
        {
            new MessagingApp.Models.Course("Web Programming", instructorId),
            new MessagingApp.Models.Course("C# Programming", instructorId),
            new MessagingApp.Models.Course("Mobile Development", instructorId),
            new MessagingApp.Models.Course("User Experience", instructorId),
            new MessagingApp.Models.Course("Programming Concepts II", instructorId),
            new MessagingApp.Models.Course("Database:SQL", instructorId)
        });
        context.SaveChanges();
    }

    if (!context.Enrollments.Any())
    {
        if (context.Users.Any() && context.Courses.Any())
        {
            var students = context.Users.Where(u => u.UserType == "student").ToList();
            var courses = context.Courses.ToList();

            context.Enrollments.AddRange(new List<MessagingApp.Models.Enrollment>
            {
                new MessagingApp.Models.Enrollment(students[0].UserId, courses[0].CourseId),
                new MessagingApp.Models.Enrollment(students[0].UserId, courses[2].CourseId),
                new MessagingApp.Models.Enrollment(students[0].UserId, courses[3].CourseId),
                new MessagingApp.Models.Enrollment(students[0].UserId, courses[4].CourseId),

                new MessagingApp.Models.Enrollment(students[1].UserId, courses[0].CourseId),
                new MessagingApp.Models.Enrollment(students[1].UserId, courses[2].CourseId),
                new MessagingApp.Models.Enrollment(students[1].UserId, courses[3].CourseId),
                new MessagingApp.Models.Enrollment(students[1].UserId, courses[4].CourseId),

                new MessagingApp.Models.Enrollment(students[2].UserId, courses[0].CourseId),
                new MessagingApp.Models.Enrollment(students[2].UserId, courses[2].CourseId),
                new MessagingApp.Models.Enrollment(students[2].UserId, courses[3].CourseId),
                new MessagingApp.Models.Enrollment(students[2].UserId, courses[4].CourseId),

                new MessagingApp.Models.Enrollment(students[3].UserId, courses[0].CourseId),
                new MessagingApp.Models.Enrollment(students[3].UserId, courses[2].CourseId),
                new MessagingApp.Models.Enrollment(students[3].UserId, courses[3].CourseId),
                new MessagingApp.Models.Enrollment(students[3].UserId, courses[5].CourseId),

                new MessagingApp.Models.Enrollment(students[4].UserId, courses[0].CourseId),
                new MessagingApp.Models.Enrollment(students[4].UserId, courses[1].CourseId),
                new MessagingApp.Models.Enrollment(students[4].UserId, courses[2].CourseId),
                new MessagingApp.Models.Enrollment(students[4].UserId, courses[3].CourseId),
                new MessagingApp.Models.Enrollment(students[4].UserId, courses[5].CourseId),

                new MessagingApp.Models.Enrollment(students[5].UserId, courses[0].CourseId),
                new MessagingApp.Models.Enrollment(students[5].UserId, courses[1].CourseId),
                new MessagingApp.Models.Enrollment(students[5].UserId, courses[2].CourseId),
                new MessagingApp.Models.Enrollment(students[5].UserId, courses[3].CourseId),
                new MessagingApp.Models.Enrollment(students[5].UserId, courses[5].CourseId),

                new MessagingApp.Models.Enrollment(students[6].UserId, courses[0].CourseId),
                new MessagingApp.Models.Enrollment(students[6].UserId, courses[1].CourseId),
                new MessagingApp.Models.Enrollment(students[6].UserId, courses[2].CourseId),
                new MessagingApp.Models.Enrollment(students[6].UserId, courses[3].CourseId),
                new MessagingApp.Models.Enrollment(students[6].UserId, courses[5].CourseId),

                new MessagingApp.Models.Enrollment(students[7].UserId, courses[0].CourseId),
                new MessagingApp.Models.Enrollment(students[7].UserId, courses[1].CourseId),
                new MessagingApp.Models.Enrollment(students[7].UserId, courses[2].CourseId),
                new MessagingApp.Models.Enrollment(students[7].UserId, courses[3].CourseId),
                new MessagingApp.Models.Enrollment(students[7].UserId, courses[5].CourseId),
            });

            context.SaveChanges();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using MessagingApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using MessagingApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure SQL Server 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";   // Redirect to login if not authenticated
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

    // Seed Users if none exist.
    if (!context.Users.Any())
    {
        context.Users.AddRange(new List<User>
        {
            new User("Austin Brown", "Abrown9034@conestogac.on.ca", "password1", "student"),
            new User("Khemara Koeun", "Koeun8402@conestogac.on.ca", "password2", "student"),
            new User("Amanda Esteves", "Aesteves3831@conestogac.on.ca", "password3", "student"),
            new User("Tristan Lagace", "Tlagace9030@conestogac.on.ca", "password4", "student")
        });
        context.SaveChanges();
    }
}

app.Run();

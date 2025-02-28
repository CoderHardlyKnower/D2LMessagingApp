using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MessagingApp.Data;
using MessagingApp.Models;
using System.Security.Claims;

namespace MessagingApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        //Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Lookup user dynamically from the database using email.
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user != null && user.Password == password)
            {
                // Create claims: use full name for display (ClaimTypes.Name)
                // and store the email in a separate "Email" claim.
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Name),  // For display in header
            new Claim("Email", user.Email),           // For filtering in CoursesController
            new Claim("UserId", user.UserId.ToString()),
            new Claim("UserType", user.UserType ?? "")
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true // Simulate persistent login.
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            // If login fails, add an error and return to the view.
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View();
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MessagingApp.Controllers
{
    public class AccountController : Controller
    {
        // A simple hardcoded dictionary of users and passwords.
        // In our simulated environment, these represent team member accounts.
        private readonly Dictionary<string, string> validUsers = new Dictionary<string, string>
        {
            {"Austin", "password1"},
            {"Khemara", "password2"},
            {"Amanda", "password3"},
            {"Tristan", "password4"}
        };

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Check against the hardcoded users.
            if (validUsers.ContainsKey(username) && validUsers[username] == password)
            {
                // Create claims for the authenticated user.
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username)
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

                // Redirect to Home (or a default authenticated page).
                return RedirectToAction("Index", "Home");
            }

            // If login fails, show an error.
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

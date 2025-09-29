using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace MessagingApp.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // If already signed in, bounce to destination (Home by default)
            if (User?.Identity?.IsAuthenticated == true)
                return Redirect(string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Index", "Home")! : returnUrl);

            // Challenge OIDC; on success, return to requested URL (or Home)
            var props = new AuthenticationProperties
            {
                RedirectUri = string.IsNullOrWhiteSpace(returnUrl) ? Url.Action("Index", "Home")! : returnUrl
            };
            return Challenge(props, OpenIdConnectDefaults.AuthenticationScheme);
        }

        // POST: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index", "Home")
            };
            return SignOut(props,
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View(); // Optional Razor view that says "You don't have access."
        }

        // GET: /Account/Me  (handy for testing)
        [Authorize]
        [HttpGet]
        public IActionResult Me()
        {
            var claims = User.Claims
                .Select(c => new { c.Type, c.Value })
                .OrderBy(c => c.Type)
                .ToList();
            return Json(new {
                name = User.Identity?.Name,
                authenticated = User.Identity?.IsAuthenticated ?? false,
                claims
            });
        }
    }
}

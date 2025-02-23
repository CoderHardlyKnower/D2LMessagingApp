using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MessagingApp.Controllers
{
    /// <summary>
    /// HomeController jsut handles redirection and basic error handling
    /// Its main purpose is to redirect the user to the course selection page
    /// </summary>

    public class HomeController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return RedirectToAction("LandingPage", "Courses");
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

    }
}

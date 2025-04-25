using Microsoft.AspNetCore.Mvc;
using PUTP2.Models;
using System.Diagnostics;

namespace PUTP2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View("WelcomeSplash");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Splash()
        {
            return View();
        }

        public IActionResult TuneInSplash()
        {
            return View();
        }
    }
}

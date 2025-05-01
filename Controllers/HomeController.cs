using IT15_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IT15_Project.Controllers
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
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Ride()
        {
            return View();
        }
        public IActionResult Rental()
        {
            return View();
        }
        public IActionResult DriverReg()
        {
            return View();
        }
        public IActionResult DriverIntro()
        {
            return View();
        }
        public IActionResult RentOwner()
        {
            return View();
        }
        public IActionResult RentIntro()
        {
            return View();
        }
        public IActionResult Admin()
        {
            return View();
        }
        public IActionResult Users()
        {
            return View();
        }
        public IActionResult PayEarn()
        {
            return View();
        }
        public IActionResult RateReview()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

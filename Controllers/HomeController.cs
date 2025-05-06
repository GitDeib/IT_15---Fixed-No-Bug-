using IT15_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IT15_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            List<string> roles = new List<string>();

            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);
                roles = (List<string>)await _userManager.GetRolesAsync(user);
            }

            ViewData["Roles"] = roles;
            return View();
        }

        public IActionResult Privacy() => View();
        public IActionResult Ride() => View();
        public IActionResult Rental() => View();
        public IActionResult DriverReg() => View();
        public IActionResult DriverIntro() => View();

        [Authorize]
        public IActionResult Admin() => View();
        [Authorize]
        public IActionResult Users() => View();
        [Authorize]
        public IActionResult PayEarn() => View();
        [Authorize]
        public IActionResult RateReview() => View();
        public IActionResult Driver() => View();
        public IActionResult RideMotor() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

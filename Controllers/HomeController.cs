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
        [Authorize(Roles = "Passenger")]
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

        /*Passenger Routes*/
        [Authorize(Roles = "Passenger")]
        public IActionResult Ride() => View();
        [Authorize(Roles = "Passenger")]
        public IActionResult RideMotor() => View();
        public IActionResult DriverReg() => View();

        public IActionResult DriverIntro() => View();
        public IActionResult Privacy() => View();



        /*Admin Routes*/
        [Authorize(Roles = "Admin")]
        public IActionResult Admin() => View();
        [Authorize(Roles = "Admin")]
        public IActionResult Users() => View();
        [Authorize(Roles = "Admin")]
        public IActionResult PayEarn() => View();
        [Authorize(Roles = "Admin")]
        public IActionResult RateReview() => View();

        /*Driver Routes*/
        [Authorize(Roles = "Driver")]
        public IActionResult Driver() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

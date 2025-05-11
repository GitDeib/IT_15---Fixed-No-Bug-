using IT15_Project.Data;
using IT15_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Encodings.Web;

namespace IT15_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;


        public HomeController(
            ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
             ApplicationDbContext context,
             IEmailSender emailSender)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailSender = emailSender;

        }
//-------------------------------------------------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------------------------------------------------
        /*Passenger Routes*/
        /*Passenger Landing*/
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

//-------------------------------------------------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------------------------------------------------


        /*Admin Routes*/
        /*Dashboard*/
        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            var pendingDriversCount = _context.Drivers.Count(d => d.Status == "Not Verified");
            var approvedDriversCount = _context.Drivers.Count(d => d.Status == "Verified");


            ViewBag.PendingDriversCount = pendingDriversCount;
            ViewBag.approvedDriversCount = approvedDriversCount;
            return View();
        }

//-------------------------------------------------------------------------------------------------------------------------------
        /*User Management*/
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Users()
        {
            var drivers = await _context.Drivers
                .Include(d => d.ApplicationUser)
                .ToListAsync();

            var passengers = await _userManager.GetUsersInRoleAsync("Passenger");

            var viewModel = new UserManagementViewModel
            {
                Drivers = drivers,
                Passengers = passengers.ToList()
            };

            return View(viewModel);
        }
        /*Delete Account Passenger*/
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "User deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Something went wrong while deleting the user.";
            }

            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> ApproveDriver(int DriverId, string? RejectionReason, string? action)
        {
            var driver = await _context.Drivers
                .Include(d => d.ApplicationUser)
                .FirstOrDefaultAsync(d => d.Id == DriverId);

            if (driver == null) return NotFound();

            var user = driver.ApplicationUser;

            if (action == "Reject")
            {
                var subject = "Driver Application Rejected";
                var message = $@"
            Hi {user.FirstName},<br><br>
            We regret to inform you that your application to become a driver has been <strong>rejected</strong>.<br><br>
            <b>Reason:</b> {HtmlEncoder.Default.Encode(RejectionReason ?? "Not specified")}<br><br>
            Thank you for your interest.<br><br>
            — SundoGo";

                await _emailSender.SendEmailAsync(user.Email, subject, message);
            }
            else
            {
                // Approve: Update status
                driver.Status = "Verified";
                _context.Update(driver);
                await _context.SaveChangesAsync();

                var subject = "Driver Application Approved";
                var message = $@"
            Hi {user.FirstName},<br><br>
            Congratulations! Your application has been <strong>approved</strong> and your account is now verified.<br><br>
            You can now log in and start accepting bookings.<br><br>
            — SundoGo";

                await _emailSender.SendEmailAsync(user.Email, subject, message);
            }

            return RedirectToAction("Users");
        }


        //-------------------------------------------------------------------------------------------------------------------------------
        /*Fare Settings*/
        [Authorize(Roles = "Admin")]
        public IActionResult PayEarn() => View();
 
 //-------------------------------------------------------------------------------------------------------------------------------       
        /*Reviews*/
        [Authorize(Roles = "Admin")]
        public IActionResult RateReview() => View();


//-------------------------------------------------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------------------------------------------------
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

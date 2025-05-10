using IT15_Project.Data;
using IT15_Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace IT15_Project.Views.Home
{
    public class AdminModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminModel> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;

        public AdminModel(
            UserManager<ApplicationUser> userManager,
            ILogger<AdminModel> logger,
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public int PendingDriversCount { get; set; }

        public class InputModel
        {
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var pendingDriversCount = await _context.Drivers.CountAsync(d => d.Status == "Not Verified");

            // Store the count in ViewData
            ViewData["PendingDriversCount"] = pendingDriversCount;

            return Page();
        }



    }
}
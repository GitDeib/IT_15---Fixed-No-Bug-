using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using IT15_Project.Data; // ← Add your actual DbContext namespace
using IT15_Project.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IT15_Project.Areas.Identity.Pages.Account.Manage
{
    public class DriverApplicationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DriverApplicationModel> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context; // ← Your DbContext

        public DriverApplicationModel(
            UserManager<ApplicationUser> userManager,
            ILogger<DriverApplicationModel> logger,
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

        public class InputModel
        {
            [Required]
            [Display(Name = "Vehicle Type")]
            public string VehicleType { get; set; }

            [Display(Name = "Vehicle Seat")]
            public string? VehicleSeat { get; set; }

            [Required]
            [Display(Name = "Vehicle Model")]
            public string VehicleModel { get; set; }

            [Required]
            [Display(Name = "Plate Number")]
            public string PlateNumber { get; set; }

            [Display(Name = "Status")]
            public string? Status { get; set; }

            [FileExtensions(Extensions = "jpg,jpeg,png", ErrorMessage = "Only image files are allowed.")]
            public string? DriverLicense { get; set; }

            [FileExtensions(Extensions = "jpg,jpeg,png", ErrorMessage = "Only image files are allowed.")]
            public string? CertificateRegistration { get; set; }

            [FileExtensions(Extensions = "jpg,jpeg,png", ErrorMessage = "Only image files are allowed.")]
            public string? OfficialReceipt { get; set; }

            [FileExtensions(Extensions = "jpg,jpeg,png", ErrorMessage = "Only image files are allowed.")]
            public string? NBIClearance { get; set; }
            public IFormFile? DriverLicensePath { get; set; }
            public IFormFile? CertificateRegistrationPath { get; set; }
            public IFormFile? OfficialReceiptPath { get; set; }
            public IFormFile? NBIClearancePath { get; set; }

        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (driver != null)
            {
                Input = new InputModel
                {
                    VehicleType = driver.VehicleType,
                    VehicleSeat = driver.VehicleSeat,
                    VehicleModel = driver.VehicleModel,
                    PlateNumber = driver.PlateNumber,
                    DriverLicense = driver.VehicleModel,
                    CertificateRegistration = driver.PlateNumber,
                    OfficialReceipt = driver.VehicleModel,
                    NBIClearance = driver.PlateNumber,
                    Status = driver.Status
                };
            }
            else
            {
                Input = new InputModel(); // no data yet
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == user.Id);

            if (driver == null)
            {
                // First time: create a new Driver record
                driver = new Driver
                {
                    UserId = user.Id,
                    VehicleType = Input.VehicleType,
                    VehicleSeat = Input.VehicleSeat,
                    VehicleModel = Input.VehicleModel,
                    PlateNumber = Input.PlateNumber,
                };
                _context.Drivers.Add(driver);
            }
            else
            {
                // Update existing record
                driver.VehicleType = Input.VehicleType;
                driver.VehicleSeat = Input.VehicleSeat;
                driver.VehicleModel = Input.VehicleModel;
                driver.PlateNumber = Input.PlateNumber;
            }

            // Save files if uploaded and replace old ones
            if (Input.DriverLicensePath != null)
            {
                if (!string.IsNullOrEmpty(driver.DriverLicense))
                {
                    DeleteFile(driver.DriverLicense);
                }
                driver.DriverLicense = await SaveFileAsync(Input.DriverLicensePath, "driverLicenses");
            }

            if (Input.CertificateRegistrationPath != null)
            {
                if (!string.IsNullOrEmpty(driver.CertificateRegistration))
                {
                    DeleteFile(driver.CertificateRegistration);
                }
                driver.CertificateRegistration = await SaveFileAsync(Input.CertificateRegistrationPath, "certificateRegistrations");
            }

            if (Input.OfficialReceiptPath != null)
            {
                if (!string.IsNullOrEmpty(driver.OfficialReceipt))
                {
                    DeleteFile(driver.OfficialReceipt);
                }
                driver.OfficialReceipt = await SaveFileAsync(Input.OfficialReceiptPath, "officialReceipts");
            }

            if (Input.NBIClearancePath != null)
            {
                if (!string.IsNullOrEmpty(driver.NBIClearance))
                {
                    DeleteFile(driver.NBIClearance);
                }
                driver.NBIClearance = await SaveFileAsync(Input.NBIClearancePath, "nbiClearances");
            }


            await _context.SaveChangesAsync();
            StatusMessage = "Driver Application updated.";
            return RedirectToPage();
        }

        private async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            // Get file extension
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            // Generate a unique file name
            var fileName = Guid.NewGuid().ToString() + fileExtension;

            // Define the path to save the file (inside wwwroot/uploads/drivers)
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "drivers");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Define the full file path
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the relative file path (to store in the database)
            return Path.Combine(fileName);
        }

        private void DeleteFile(string fileName)
        {
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "drivers", fileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }


    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using IT15_Project.Data;
using IT15_Project.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace IT15_Project.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class DriverRegModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DriverRegModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;


        public DriverRegModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DriverRegModel> logger,
            IEmailSender emailSender,
            IWebHostEnvironment env,
            ApplicationDbContext context) // Ensure ApplicationDbContext is passed here

        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _env = env;
            _context = context;  // Initialize the context

        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Display(Name = "Middle Name")]
            public string MiddleName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Phone]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Required]
            public string Address { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Display(Name = "Profile Photo")]
            public IFormFile? ProfilePhoto { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [Display(Name = "Vehicle Type")]
            public string VehicleType { get; set; }

            [Display(Name = "Vehicle Seat Type")]
            public string VehicleSeat { get; set; }

            [Required]
            [Display(Name = "Vehicle Model")]
            public string VehicleModel { get; set; }

            [Required]
            [Display(Name = "Plate Number")]
            public string PlateNumber { get; set; }

            [Required]
            [Display(Name = "Driver's License")]
            public IFormFile DriverLicense { get; set; }

            [Required]
            [Display(Name = "CR (Certificate of Registration)")]
            public IFormFile CertificateRegistration { get; set; }

            [Required]
            [Display(Name = "OR (Official Receipt)")]
            public IFormFile OfficialReceipt { get; set; }

            [Required]
            [Display(Name = "NBI Clearance")]
            public IFormFile NBIClearance { get; set; }

            [Required]
            [Display(Name = "Motivation")]
            public string Motivation { get; set; }

            public string Status { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Save uploaded files
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "drivers");
                Directory.CreateDirectory(uploadsFolder);

                string SaveFile(IFormFile file)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    return fileName;
                }

                var licensePath = SaveFile(Input.DriverLicense);
                var crPath = SaveFile(Input.CertificateRegistration);
                var orPath = SaveFile(Input.OfficialReceipt);
                var nbiPath = SaveFile(Input.NBIClearance);

                string? photoPath = null;

                if (Input.ProfilePhoto != null)
                {
                    var profilePhotosFolder = Path.Combine(_env.WebRootPath, "uploads/profilephotos");
                    Directory.CreateDirectory(profilePhotosFolder); // Make sure folder exists

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.ProfilePhoto.FileName);
                    var filePath = Path.Combine(profilePhotosFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Input.ProfilePhoto.CopyToAsync(stream);
                    }

                    photoPath = fileName;
                }

                // Create the ApplicationUser
                var user = new ApplicationUser
                {
                    FirstName = Input.FirstName,
                    MiddleName = Input.MiddleName,
                    LastName = Input.LastName,
                    PhoneNumber = Input.PhoneNumber,
                    Address = Input.Address,
                    UserName = Input.Email,
                    Email = Input.Email,
                    ProfilePhotoPath = photoPath,
                    CreatedAt = DateTime.Now,
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    // Add the driver role
                    await _userManager.AddToRoleAsync(user, "Driver");

                    // Now create and save the Driver record
                    var driver = new Driver
                    {
                        UserId = user.Id,
                        VehicleType = Input.VehicleType,
                        VehicleSeat = Input.VehicleSeat,
                        VehicleModel = Input.VehicleModel,
                        PlateNumber = Input.PlateNumber,
                        DriverLicense = licensePath,
                        CertificateRegistration = crPath,
                        OfficialReceipt = orPath,
                        NBIClearance = nbiPath,
                        Motivation = Input.Motivation,
                        Status = "Not Verified" // <-- Added this line

                    };

                    _context.Drivers.Add(driver);  // Save the driver record
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User created a new driver account with password.");

                    // Email confirmation logic
                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId, code, returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $@"
                        <html>
                          <head></head>
                          <body style='font-family: Arial, sans-serif; background-color: #f8f9fa; margin: 0; padding: 0;'>
                            <div style='display: flex; justify-content: center; align-items: center; min-height: 100vh;'>
                              <div style='max-width: 600px; margin: 20px auto; padding: 40px; background-color: white; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); text-align: left;'>
                                <h2 style='color: #343a40;'>Driver Registration Confirmation</h2>
                                <p style='font-size: 16px; color: #495057;'>Dear {Input.FirstName},</p>
                                <p style='font-size: 16px; color: #495057;'>
                                  Thank you for applying as a driver! Our team will review your application.
                                </p>
                                <p style='font-size: 16px; color: #495057;'>
                                  You will be notified via your contact number or email once your application is approved.
                                </p>
                                <p style='font-size: 16px; color: #495057;'>
                                  To complete your registration, please confirm your email by clicking the button below.
                                </p>
                                <div style='text-align: center; margin: 30px 0;'>
                                  <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'
                                     style='display: inline-block; padding: 12px 24px; background-color: #343a40; color: #fff; text-decoration: none; border-radius: 4px; font-weight: bold;'>
                                    Confirm Your Email
                                  </a>
                                </div>
                                <p style='font-size: 14px; color: #868e96;'>If you did not request this registration, you can safely ignore this email.</p>
                                <p style='font-size: 14px; color: #868e96;'>— The Team</p>
                              </div>
                            </div>
                          </body>
                        </html>
                        ");


                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }


        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}

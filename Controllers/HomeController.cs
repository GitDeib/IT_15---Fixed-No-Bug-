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
        public async Task<IActionResult> Ride()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index");
            }

            // Get active booking if exists
            var activeBooking = await _context.Bookings
                .Include(b => b.FareSetting)
                .Include(b => b.Driver)
                .ThenInclude(b => b.DriverProfile)
                .Where(b => b.UserId == userId &&
                    (b.Status == BookingStatus.Pending ||
                     b.Status == BookingStatus.Accepted ||
                     b.Status == BookingStatus.Started ||
                     (b.Status == BookingStatus.Completed && b.PaymentStatus == PaymentStatus.Unpaid)))
                .OrderByDescending(b => b.RequestedAt)
                .FirstOrDefaultAsync();

            ViewBag.ActiveBooking = activeBooking;
            return View();
        }


        public async Task<IActionResult> GetFareDetails(string seatType)
        {
            if (string.IsNullOrEmpty(seatType))
            {
                return Json(new { error = "Seat type is required." });
            }

            var fareSetting = await _context.FareSettings
                                            .FirstOrDefaultAsync(f => f.SeatType == seatType);

            if (fareSetting == null)
            {
                return Json(new { error = "Fare details not found for selected seat type." });
            }

            return Json(new
            {
                BaseFare = fareSetting.BaseFare,
                PerKilometerRate = fareSetting.PerKilometerRate,
                PerMinuteRate = fareSetting.PerMinuteRate
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ride(Booking model)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "You must be logged in to book a ride.";
                return RedirectToAction("Ride");
            }

            model.UserId = userId;

            if (ModelState.IsValid)
            {
                var fareSetting = await _context.FareSettings.FindAsync(model.FareSettingsId);
                if (fareSetting == null)
                {
                    TempData["Error"] = "Invalid fare setting selected.";
                    return RedirectToAction("Ride");
                }

                model.Status = BookingStatus.Pending;
                model.RequestedAt = DateTime.UtcNow;

                _context.Bookings.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Booking successfully created!";
                return RedirectToAction("Ride");
            }

            TempData["Error"] = string.Join(" | ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return RedirectToAction("Ride");
        }

        //Cancel booking 
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Passenger")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index");
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("Ride");
            }

            if (booking.Status != BookingStatus.Pending)
            {
                TempData["Error"] = "Only pending bookings can be cancelled.";
                return RedirectToAction("Ride");
            }

            booking.Status = BookingStatus.Cancelled;
            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking cancelled successfully.";
            return RedirectToAction("Ride");
        }


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


        //Approval and Reject Driver
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
        public IActionResult PayEarn()
        {
            var model = new FareSettingsViewModel
            {
                Fare4 = _context.FareSettings.FirstOrDefault(f => f.SeatType == "4-seater"),
                Fare6 = _context.FareSettings.FirstOrDefault(f => f.SeatType == "6-seater"),
                Fare1 = _context.FareSettings.FirstOrDefault(f => f.SeatType == "1-seater")
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateCarFare(decimal BaseFare4Seater, decimal PerKilometerRate4Seater, decimal BaseFare6Seater, decimal PerKilometerRate6Seater, decimal PerMinuteRate, int DriverShareRate, int CommissionRate)
        {
            // Fetching the fare settings for both 4-seater and 6-seater
            var fare4Seater = await _context.FareSettings.FirstOrDefaultAsync(f => f.SeatType == "4-Seater");
            var fare6Seater = await _context.FareSettings.FirstOrDefaultAsync(f => f.SeatType == "6-Seater");

            if (fare4Seater == null || fare6Seater == null)
            {
                return NotFound("Fare settings not found.");
            }

            // Update 6-Seater fare settings
            fare6Seater.BaseFare = BaseFare6Seater;
            fare6Seater.PerKilometerRate = PerKilometerRate6Seater;
            fare6Seater.PerMinuteRate = PerMinuteRate;
            fare6Seater.DriverShareRate = DriverShareRate;
            fare6Seater.CommissionRate = CommissionRate;

            // Update 4-Seater fare settings
            fare4Seater.BaseFare = BaseFare4Seater;
            fare4Seater.PerKilometerRate = PerKilometerRate4Seater;
            fare4Seater.PerMinuteRate = fare6Seater.PerMinuteRate; // Copy from 6-Seater
            fare4Seater.DriverShareRate = fare6Seater.DriverShareRate; // Copy from 6-Seater
            fare4Seater.CommissionRate = fare6Seater.CommissionRate; // Copy from 6-Seater

            // Save changes to the database
            try
            {
                _context.FareSettings.Update(fare4Seater);
                _context.FareSettings.Update(fare6Seater);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong while updating fare settings.");
                return View(); // or return to the current view with an error message
            }

            TempData["Success"] = "Fare settings updated successfully.";
            return RedirectToAction("PayEarn");
        }



        //Motor Update
        [HttpPost]
        public async Task<IActionResult> UpdateMotorFare(int Id, decimal BaseFare, decimal PerKilometerRate, decimal PerMinuteRate, int DriverShareRate, int CommissionRate)
        {
            var fareSetting = await _context.FareSettings.FindAsync(Id);
            if (fareSetting == null)
            {
                return NotFound();
            }

            // Update the fare setting values
            fareSetting.BaseFare = BaseFare;
            fareSetting.PerKilometerRate = PerKilometerRate;
            fareSetting.PerMinuteRate = PerMinuteRate;
            fareSetting.DriverShareRate = DriverShareRate;
            fareSetting.CommissionRate = CommissionRate;

            // Save changes to database
            try
            {
                _context.FareSettings.Update(fareSetting);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log or handle the error here
                ModelState.AddModelError("", "Something went wrong while updating fare settings.");
                return View(); // or return to the current view with error
            }

            TempData["Success"] = "Motorcycle fare updated successfully.";
            return RedirectToAction("PayEarn"); // or your dashboard or fare settings page
        }

        //-------------------------------------------------------------------------------------------------------------------------------       
        /*Reviews*/
        [Authorize(Roles = "Admin")]
        public IActionResult RateReview() => View();


        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;

            await _userManager.UpdateAsync(user);
            TempData["Message"] = "User account has been deactivated.";

            return RedirectToAction("Users"); // Replace with your actual view name
        }*/



        //-------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> Driver()
        {
            var userId = _userManager.GetUserId(User);

            var driver = await _context.Drivers
                .Include(d => d.ApplicationUser)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (driver == null)
                return NotFound("Driver not found.");

            var fareSetting = await _context.FareSettings.FirstOrDefaultAsync();

            // Get current active booking if exists (either accepted, started, or completed but unpaid)
            var activeBooking = await _context.Bookings
                .Include(b => b.User)
                .Where(b => b.DriverId == userId &&
                    (b.Status == BookingStatus.Accepted || 
                     b.Status == BookingStatus.Started || 
                     (b.Status == BookingStatus.Completed && b.PaymentStatus == PaymentStatus.Unpaid)))
                .OrderByDescending(b => b.RequestedAt)
                .FirstOrDefaultAsync();

            // Filter pending bookings based on the driver's seat type
            var availablePassengers = await _context.Bookings
                .Include(b => b.User)
                .Where(b => b.Status == BookingStatus.Pending && b.VehicleSeat == driver.VehicleSeat)
                .ToListAsync();

            var model = new DriverDashboardViewModel
            {
                Driver = driver,
                FareSetting = fareSetting,
                ActiveBooking = activeBooking,
                AvailablePassengers = availablePassengers,
                IsAvailable = driver.Status == "Available"
            };

            ViewBag.ActiveBooking = activeBooking;
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> AcceptBooking(int bookingId)
        {
            var userId = _userManager.GetUserId(User);

            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);
            if (driver == null)
                return NotFound("Driver not found.");

            // Check if the driver is verified
            if (driver.Status != "Verified")
            {
                TempData["Error"] = "Your account is not verified. You cannot accept bookings.";
                return RedirectToAction("Driver");
            }

            // Check if driver already has an active booking
            var hasActiveBooking = await _context.Bookings
                .AnyAsync(b => b.DriverId == userId && 
                    (b.Status == BookingStatus.Accepted || 
                     b.Status == BookingStatus.Started || 
                     (b.Status == BookingStatus.Completed && b.PaymentStatus == PaymentStatus.Unpaid)));

            if (hasActiveBooking)
            {
                TempData["Error"] = "You already have an active booking. Please complete it first.";
                return RedirectToAction("Driver");
            }

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null)
                return NotFound("Booking not found.");

            if (booking.Status != BookingStatus.Pending)
            {
                TempData["Error"] = "This booking is no longer available.";
                return RedirectToAction("Driver");
            }

            booking.Status = BookingStatus.Accepted;
            booking.DriverId = userId;
/*            booking.AcceptedAt = DateTime.UtcNow;
*/
            await _context.SaveChangesAsync();
            TempData["Success"] = "Booking accepted successfully!";
            return RedirectToAction("Driver");
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> StartRide(int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.DriverId == userId);

            if (booking == null)
                return NotFound();

            if (booking.Status != BookingStatus.Accepted)
            {
                TempData["Error"] = "Invalid booking status.";
                return RedirectToAction("Driver");
            }

            booking.Status = BookingStatus.Started;
            booking.StartedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Ride started successfully!";
            return RedirectToAction("Driver");
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> CompleteRide(int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings
                .Include(b => b.FareSetting)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.DriverId == userId);

            if (booking == null)
                return NotFound();

            if (booking.Status != BookingStatus.Started)
            {
                TempData["Error"] = "Invalid booking status.";
                return RedirectToAction("Driver");
            }

            // Calculate actual time taken
            var actualTimeInMinutes = (DateTime.UtcNow - booking.StartedAt.Value).TotalMinutes;
            booking.ActualTimeInMinutes = actualTimeInMinutes;

            // Calculate final fare
            var fareSetting = booking.FareSetting;
            if (fareSetting != null)
            {
                // Base fare + (distance * per km rate) + (actual time * per minute rate)
                var finalFare = fareSetting.BaseFare +
                               (decimal)(booking.DistanceInKm * (double)fareSetting.PerKilometerRate) +
                               (decimal)(actualTimeInMinutes * (double)fareSetting.PerMinuteRate);

                booking.FinalFare = Math.Round(finalFare, 2);
            }

            booking.Status = BookingStatus.Completed;
            booking.CompletedAt = DateTime.UtcNow;
            booking.PaymentStatus = PaymentStatus.Unpaid; // Set initial payment status as unpaid

            await _context.SaveChangesAsync();
            TempData["Success"] = "Ride completed! Waiting for passenger payment.";
            return RedirectToAction("Driver");
        }



    }


}

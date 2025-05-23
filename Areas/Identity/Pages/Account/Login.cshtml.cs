﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using IT15_Project.Models;

namespace IT15_Project.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    var user = await _userManager.FindByNameAsync(Input.Email);
                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);

                        if (roles.Contains("Admin"))
                        {
                            return LocalRedirect("/Home/Admin");
                        }
                        else if (roles.Contains("Passenger"))
                        {
                            return LocalRedirect("/Home/Index");
                        }
                        else if (roles.Contains("Driver"))
                        {
                            return LocalRedirect("/Home/Driver");
                        }
                    }

                    return LocalRedirect(returnUrl); // fallback
                }

            
            if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);

                    if (lockoutEnd.HasValue)
                    {
                        var remaining = lockoutEnd.Value.UtcDateTime - DateTime.UtcNow;
                        var timeLeft = remaining.TotalMinutes < 1
                            ? $"{remaining.Seconds} seconds"
                            : $"{remaining.Minutes} minutes and {remaining.Seconds % 60} seconds";

                        ModelState.AddModelError("", $"Your account is locked. Please try again in {timeLeft}.");
                    }
                }
                else
                {
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    if (user != null && await _userManager.GetLockoutEnabledAsync(user))
                    {
                        int accessFailedCount = await _userManager.GetAccessFailedCountAsync(user);
                        int maxAttempts = _userManager.Options.Lockout.MaxFailedAccessAttempts;

                        int remainingAttempts = maxAttempts - accessFailedCount;

                        if (remainingAttempts > 0)
                        {
                            ModelState.AddModelError(string.Empty, $"You have {remainingAttempts} more attempt(s) before your account gets locked.");
                        }
                    }
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}

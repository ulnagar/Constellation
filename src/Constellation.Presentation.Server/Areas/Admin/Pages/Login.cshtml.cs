﻿using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Admin.Pages
{
    [AllowAnonymous]
    public class LoginModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<IAuthService> _logger;
        private readonly SignInManager<AppUser> _signInManager;

        public LoginModel(SignInManager<AppUser> signInManager, IUnitOfWork unitOfWork, UserManager<AppUser> userManager, ILogger<IAuthService> logger)
            : base()
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
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

        private class LoginResult
        {
            public bool Succeeded { get; set; }
            public bool RequiresTwoFactor { get; set; }
            public bool IsLockedOut { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            await GetClasses(_unitOfWork);

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
                var localAuth = Input.Email.Contains("~");
                Input.Email = Input.Email.Replace("~", "");

                _logger.LogInformation($"Starting Login Attempt by {Input.Email}");

                var user = await _userManager.FindByEmailAsync(Input.Email);

                if (user == null)
                {
                    _logger.LogWarning($" - No user found for email {Input.Email}");

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                } else
                {
                    _logger.LogInformation($" - Found user {user.Id} for email {Input.Email}");
                }

                var result = new LoginResult();

                if (!localAuth)
                {
                    _logger.LogInformation($" - Attempting domain login by {Input.Email}");

#pragma warning disable CA1416 // Validate platform compatibility
                    var context = new PrincipalContext(ContextType.Domain, "DETNSW.WIN");
                    var success = context.ValidateCredentials(Input.Email, Input.Password);
#pragma warning restore CA1416 // Validate platform compatibility

                    if (!success)
                    {
                        _logger.LogWarning($" - Domain login failed for {Input.Email}");

                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    } else
                    {
                        _logger.LogInformation($" - Domain login succeeded for {Input.Email}");
                    }

                    await _signInManager.SignInAsync(user, false);
                    result.Succeeded = true;
                } else
                {
                    _logger.LogInformation($" - Attempting local login by {Input.Email}");
#if DEBUG
                    _logger.LogInformation($" - DEBUG code found. Bypass login check.");
                    await _signInManager.SignInAsync(user, false);
                    result.Succeeded = true;
#else
                    var passwordResult = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                    
                    if (!passwordResult.Succeeded)
                    {
                        _logger.LogWarning($" - Local login failed for {Input.Email}");
                    }
                    else
                    {
                        result.Succeeded = passwordResult.Succeeded;
                        _logger.LogInformation($" - Local login suceeded for {Input.Email}");
                    }
#endif
                }

                if (result.Succeeded)
                {
                    return LocalRedirect(returnUrl);
                }
                //if (result.RequiresTwoFactor)
                //{
                //    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
                //}
                //if (result.IsLockedOut)
                //{
                //    return RedirectToPage("./Lockout");
                //}
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}

using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Portal.Schools.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public LoginModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
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
        }

        private class LoginResult
        {
            public bool Succeeded { get; set; }
            public bool RequiresTwoFactor { get; set; }
            public bool IsLockedOut { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl ?? Url.Content("~/");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            
            if (ModelState.IsValid)
            {
                var localAuth = Input.Email.Contains("~");
                Input.Email = Input.Email.Replace("~", "");
                var user = await _userManager.FindByEmailAsync(Input.Email);

                var result = new LoginResult();

                if (!localAuth)
                {
#pragma warning disable CA1416 // Validate platform compatibility
                    var context = new PrincipalContext(ContextType.Domain, "DETNSW.WIN");
                    var success = context.ValidateCredentials(Input.Email, Input.Password);

                    if (success)
                    {
                        var userAccount = UserPrincipal.FindByIdentity(context, IdentityType.UserPrincipalName, Input.Email);
                        using (DirectoryEntry adAccount = userAccount.GetUnderlyingObject() as DirectoryEntry)
                        {
                            // detAttribute12 is the staff attribute that lists linked school codes
                            // detAttribute3 is the staff attribute that contains the employee number

                            var schoolList = adAccount.Properties["detAttribute12"].Value.ToString();

                            // Check each value returned against list of partner schools with students
                            // Add any matching entries to the user claims


                            
                        }
                    }
#pragma warning restore CA1416 // Validate platform compatibility

                    if (!success)
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    }

                    await _signInManager.SignInAsync(user, false);
                    result.Succeeded = true;
                }
                else
                {
#if DEBUG
                    await _signInManager.SignInAsync(user, false);
                    result.Succeeded = true;
#else
                    var passwordResult = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                    result.Succeeded = passwordResult.Succeeded;
#endif
                }

                if (result.Succeeded)
                {
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            return Page();
        }
    }
}

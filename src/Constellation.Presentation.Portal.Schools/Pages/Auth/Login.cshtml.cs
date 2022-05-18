using Constellation.Application.Features.Auth.Command;
using Constellation.Application.Features.Auth.Queries;
using Constellation.Application.Features.Portal.School.Home.Queries;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Portal.Schools.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Constellation.Presentation.Portal.Schools.Pages.Auth
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMediator _mediator;
        private readonly IActiveDirectoryActionsService _adService;
        private readonly ILogger<IAuthService> _logger;

        public LoginModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager,
            IMediator mediator, IActiveDirectoryActionsService adService,
            ILogger<IAuthService> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mediator = mediator;
            _adService = adService;
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
            [RegularExpression(@"^\w+([-+.']\w+)*@det.nsw.edu.au$", ErrorMessage = "Invalid Email.")]
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
                // Check AD for user account
                //  If exists - continue
                //  else return error
                // Search for user in database
                //  If exists - continue
                //  else check SchoolContacts for old user
                //   If exists - reactivate user and continue login
                //   else register user with data from AD and continue login

                _logger.LogInformation($"Starting Login Attempt by {Input.Email}");

#if DEBUG

                //var user = await _userManager.FindByEmailAsync(Input.Email);

                //if (user == null)
                //{
                //    _logger.LogWarning($" - No user found for email {Input.Email}");
                //    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                //    return Page();
                //}

                //_logger.LogInformation($" - Found user {user.Id} for email {Input.Email}");

                // Test with dummy claims
                var claimList = new List<Claim>();
                claimList.Add(new Claim("Schools", "8146,8155,8343"));

                var dbUserAccount = await _userManager.FindByEmailAsync(Input.Email);

                //if (dbUserAccount == null)
                //{
                //    _logger.LogWarning(" - No user found for email {user}", Input.Email);

                //    var dbContact = await _mediator.Send(new GetSchoolContactByEmailAddressQuery { EmailAddress = Input.Email });

                //    if (dbContact == null)
                //    {
                //        _logger.LogWarning(" - No Contact Record found for email {user}", Input.Email);

                //         Check that user is linked to valid school first.
                //        var adSchools = await _adService.GetLinkedSchoolsFromAD(Input.Email);

                //        if (adSchools != null && adSchools.Count > 0)
                //        {
                //            _logger.LogWarning(" - Found valid linked school in AD Records for user {user}", Input.Email);

                //             Send to registration
                //            await _mediator.Send(new RegisterADUserAsSchoolContactCommand { EmailAddress = Input.Email });
                //            dbUserAccount = await _userManager.FindByEmailAsync(Input.Email);

                //            _logger.LogWarning(" - User created for email {user}", Input.Email);
                //        }
                //        else
                //        {
                //            _logger.LogWarning(" - No valid linked school found for non-existing user {user}", Input.Email);
                //            throw new PortalAppAuthenticationException("Could not find valid Partner School to link new user with.");
                //        }
                //    }
                //    else
                //    {
                //        _logger.LogWarning(" - Found inactive user for email {user}", Input.Email);
                //         Recreate user account based on SchoolContact entry
                //        await _mediator.Send(new RepairSchoolContactUserCommand { SchoolContactId = dbContact.Id });
                //        dbUserAccount = await _userManager.FindByEmailAsync(Input.Email);

                //        _logger.LogWarning(" - Inactive user reactivated for email {user}", Input.Email);
                //    }
                //}

                // Build/rebuild schools claim
                //await _mediator.Send(new UpdateUserSchoolsClaimCommand { EmailAddress = Input.Email });

                _logger.LogInformation($" - DEBUG code found. Bypass login check.");
                await _signInManager.SignInWithClaimsAsync(dbUserAccount, false, claimList);

                return LocalRedirect(returnUrl);
#else
#pragma warning disable CA1416 // Validate platform compatibility
                _logger.LogInformation(" - Attempting domain login by {user}", Input.Email);
                var context = new PrincipalContext(ContextType.Domain, "DETNSW.WIN");
                var success = context.ValidateCredentials(Input.Email, Input.Password);

                if (!success)
                {
                    _logger.LogWarning(" - Domain login failed for {user}", Input.Email);

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

                _logger.LogInformation(" - Domain login succeeded for {user}", Input.Email);

                //var adUserAccount = UserPrincipal.FindByIdentity(context, IdentityType.UserPrincipalName, Input.Email);
                var dbUserAccount = await _userManager.FindByEmailAsync(Input.Email);

                if (dbUserAccount == null)
                {
                    _logger.LogWarning(" - No user found for email {user}", Input.Email);

                    var dbContact = await _mediator.Send(new GetSchoolContactByEmailAddressQuery { EmailAddress = Input.Email });

                    if (dbContact == null)
                    {
                        _logger.LogWarning(" - No Contact Record found for email {user}", Input.Email);

                        // Check that user is linked to valid school first.
                        var adSchools = await _adService.GetLinkedSchoolsFromAD(Input.Email);

                        if (adSchools != null && adSchools.Count > 0)
                        {
                            _logger.LogWarning(" - Found valid linked school in AD Records for user {user}", Input.Email);

                            // Send to registration
                            await _mediator.Send(new RegisterADUserAsSchoolContactCommand { EmailAddress = Input.Email });
                            dbUserAccount = await _userManager.FindByEmailAsync(Input.Email);

                            _logger.LogWarning(" - User created for email {user}", Input.Email);
                        } else
                        {
                            _logger.LogWarning(" - No valid linked school found for non-existing user {user}", Input.Email);
                            throw new PortalAppAuthenticationException("Could not find valid Partner School to link new user with.");
                        }
                    } else
                    {
                        _logger.LogWarning(" - Found inactive user for email {user}", Input.Email);
                        // Recreate user account based on SchoolContact entry
                        await _mediator.Send(new RepairSchoolContactUserCommand { SchoolContactId = dbContact.Id });
                        dbUserAccount = await _userManager.FindByEmailAsync(Input.Email);

                        _logger.LogWarning(" - Inactive user reactivated for email {user}", Input.Email);
                    }
                }

                // Build/rebuild schools claim
                await _mediator.Send(new UpdateUserSchoolsClaimCommand { EmailAddress = Input.Email });

                await _signInManager.SignInAsync(dbUserAccount, false);

                _logger.LogInformation(" - Login succeeded for {user}", Input.Email);

                return LocalRedirect(returnUrl);
#pragma warning restore CA1416 // Validate platform compatibility
#endif
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt."); 
            return Page();
        }
    }
}

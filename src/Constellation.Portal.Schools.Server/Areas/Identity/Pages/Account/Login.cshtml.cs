#nullable disable
namespace Constellation.Portal.Schools.Server.Areas.Identity.Pages.Account;

using Constellation.Application.Features.Auth.Command;
using Constellation.Application.Features.Auth.Queries;
using Constellation.Application.Features.Portal.School.Home.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

public class LoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IActiveDirectoryGateway _gateway;
    private readonly IMediator _mediator;
    private readonly ILogger<IAuthService> _logger;

    internal enum LoginStatus
    {
        WaitingUserInput,
        InvalidUsername
    }

    public LoginModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager,
        IActiveDirectoryGateway gateway,
        IMediator mediator, ILogger<IAuthService> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _gateway = gateway;
        _mediator = mediator;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ReturnUrl { get; set; }

    internal LoginStatus Status { get; set; } = LoginStatus.WaitingUserInput;

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [RegularExpression(@"^~*\w+([-+.']\w+)*@det.nsw.edu.au$", ErrorMessage = "Invalid Email.")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public async Task OnGetAsync(string returnUrl = null)
    {
        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            var localAuth = Input.Email.Contains("~");
            Input.Email = Input.Email.Replace("~", "");

            _logger.LogInformation($"Starting Login Attempt by {Input.Email}");

            if (!localAuth)
            {
                // Verify password
                var authSuccessful = await _gateway.VerifyUserCredentialsAgainstActiveDirectory(Input.Email, Input.Password);

                if (!authSuccessful)
                {
                    _logger.LogWarning(" - Domain login failed for {user}", Input.Email);

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

                _logger.LogInformation(" - Domain login succeeded for {user}", Input.Email);
            }

            // Verify that user is a School Contact, or admin
            var user = await _userManager.FindByEmailAsync(Input.Email);
            var isValid = await _mediator.Send(new IsUserASchoolContactQuery { EmailAddress = Input.Email });
            var roles = await _userManager.GetRolesAsync(user);
            var isAdmin = roles.Contains(AuthRoles.Admin);
            if (user == null && isValid)
            {
                // No user found, but there should be a user account
                _logger.LogWarning(" - Found inactive user for email {user}", Input.Email);

                var contact = await _mediator.Send(new GetSchoolContactByEmailAddressQuery { EmailAddress = Input.Email });
                await _mediator.Send(new RepairSchoolContactUserCommand { SchoolContactId = contact.Id });

                user = await _userManager.FindByEmailAsync(Input.Email);
            }
            else if (user == null && !isValid && !isAdmin)
            {
                // User has no local school contact record. New user?
                _logger.LogWarning(" - No Contact Record found for email {user}", Input.Email);

                // Check if AD lists user attached to partner school
                var adSchools = await _gateway.GetLinkedSchoolsFromActiveDirectory(Input.Email);

                if (adSchools != null && adSchools.Count > 0)
                {
                    var validSchools = await _mediator.Send(new GetAllPartnerSchoolCodesQuery());

                    // If no, return error
                    if (adSchools.Except(validSchools).Count() == adSchools.Count)
                    {
                        // There is no school listed against this user in AD that matches current partner schools

                        _logger.LogWarning(" - No valid Partner Schools listed in Active Directory for user {user}", Input.Email);

                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    }

                    // If yes, create new user account and log in
                    _logger.LogWarning(" - Found valid linked school in AD Records for user {user}", Input.Email);

                    // Send to registration
                    await _mediator.Send(new RegisterADUserAsSchoolContactCommand { EmailAddress = Input.Email });
                    user = await _userManager.FindByEmailAsync(Input.Email);

                    _logger.LogWarning(" - User created for email {user}", Input.Email);
                }
                else
                {
                    _logger.LogWarning(" - No valid linked school found for non-existing user {user}", Input.Email);

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }
            else if (user != null && isAdmin)
            {
                _logger.LogInformation($" - Admin access granted. Logging in with system wide access.");
            }

            if (localAuth)
            {
#if DEBUG
                _logger.LogInformation(" - DEBUG code found. Bypass login check.");
#else
                var passwordResult = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                    
                if (!passwordResult.Succeeded)
                {
                    _logger.LogWarning(" - Local login failed for {Email}", Input.Email);
                
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
                else
                {
                    _logger.LogInformation(" - Local login suceeded for {Email}", Input.Email);

                    return LocalRedirect(returnUrl);
                }
#endif
            }

            await _signInManager.SignInAsync(user, false);

            _logger.LogInformation(" - Login succeeded for {user}", Input.Email);

            return LocalRedirect(returnUrl);
        }

        // There was a problem with the data returned. Show an error.
        Status = LoginStatus.InvalidUsername;

        return Page();
    }
}

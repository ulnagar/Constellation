#nullable disable
namespace Constellation.Portal.Schools.Server.Areas.Identity.Pages.Account;

using Constellation.Application.Features.Auth.Command;
using Constellation.Application.Features.Auth.Queries;
using Constellation.Application.Features.Portal.School.Home.Queries;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;

public class LoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMediator _mediator;
    private readonly IActiveDirectoryActionsService _adService;
    private readonly Serilog.ILogger _logger;

    public LoginModel(
        SignInManager<AppUser> signInManager,
        Serilog.ILogger logger,
        UserManager<AppUser> userManager,
        IMediator mediator,
        IActiveDirectoryActionsService adService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _mediator = mediator;
        _adService = adService;
        _logger = logger.ForContext<IAuthService>();
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public List<AuthenticationScheme> ExternalLogins { get; set; }

    public string ReturnUrl { get; set; }

    [TempData]
    public string ErrorMessage { get; set; }

    private class LoginResult
    {
        public bool Succeeded { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public bool IsLockedOut { get; set; }
    }

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

    public async Task OnGetAsync(string returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins = new();

        ReturnUrl = returnUrl;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "System requirement for federated login")]
    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        ExternalLogins = new();

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

            _logger.Information("Starting login attempt by {user}", Input.Email);

            // Insert debug bypass code here

            _logger.Information(" - Attempting domain login by {user}", Input.Email);

            var context = new PrincipalContext(ContextType.Domain, "DETNSW.WIN");
            var success = context.ValidateCredentials(Input.Email, Input.Password);

            if (!success)
            {
                _logger.Warning(" - Domain login failed for {user}", Input.Email);

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            _logger.Information(" - Domain login succeeded for {user}", Input.Email);

            var dbUserAccount = await _userManager.FindByEmailAsync(Input.Email);

            if (dbUserAccount is null)
            {
                _logger.Warning(" - No user found for email {user}", Input.Email);

                var dbContact = await _mediator.Send(new GetSchoolContactByEmailAddressQuery { EmailAddress = Input.Email });

                if (dbContact is null)
                {
                    _logger.Warning(" - No Contact Record found for email {user}", Input.Email);

                    // Check that the user is linked to valid school first.
                    var adSchools = await _adService.GetLinkedSchoolsFromAD(Input.Email);

                    if (adSchools is not null && adSchools.Any())
                    {
                        _logger.Warning(" - Found valid linked school in AD Records for user {user}", Input.Email);

                        // Send to registration
                        await _mediator.Send(new RegisterADUserAsSchoolContactCommand { EmailAddress = Input.Email });
                        dbUserAccount = await _userManager.FindByEmailAsync(Input.Email);

                        _logger.Warning(" - User created for email {user}", Input.Email);
                    } 
                    else
                    {
                        _logger.Warning(" - No valid linked school found for non-existing user {user}", Input.Email);

                        ModelState.AddModelError(string.Empty, "Could not find valid Partner School to link new user with. Please contact Technology Support Team on 1300 610 733.");
                        return Page();
                    }
                } 
                else
                {
                    _logger.Warning(" - Found inactive user for email {user}", Input.Email);
                    // Recreate user account based on SchoolContact entry
                    await _mediator.Send(new RepairSchoolContactUserCommand { SchoolContactId = dbContact.Id });
                    dbUserAccount = await _userManager.FindByEmailAsync(Input.Email);

                    if (dbUserAccount is not null)
                    {
                        _logger.Warning(" - Inactive user reactivated for email {user}", Input.Email);
                    } else
                    {
                        _logger.Warning(" - Inactive user could not be reactivated for email {user}", Input.Email);

                        ModelState.AddModelError(string.Empty, "Could not find valid user. Please contact Technology Support Team on 1300 610 733.");
                        return Page();
                    }
                }
            }

            // Is user an admin?
            var roles = await _userManager.GetRolesAsync(dbUserAccount);
            if (roles.Contains(AuthRoles.Admin))
            {
                var schools = await _mediator.Send(new GetAllPartnerSchoolCodesQuery());
                var claimList = new List<Claim>();

                foreach (var entry in schools)
                {
                    claimList.Add(new Claim(AuthClaimType.SchoolCode, entry));
                }

                _logger.Information(" - Admin access granted. Logging in with system wide access.");
                await _signInManager.SignInWithClaimsAsync(dbUserAccount, false, claimList);

                _logger.Information(" - Login succeeded for {user}", Input.Email);
                return LocalRedirect(returnUrl);
            }

            // Build/rebuild schools claims
            await _mediator.Send(new UpdateUserSchoolsClaimCommand { EmailAddress = Input.Email });

            await _signInManager.SignInAsync(dbUserAccount, false);

            _logger.Information(" - Login succeeded for {user}", Input.Email);

            return LocalRedirect(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }
}

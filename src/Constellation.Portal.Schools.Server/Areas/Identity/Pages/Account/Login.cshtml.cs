namespace Constellation.Portal.Schools.Server.Areas.Identity.Pages.Account;

using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Exceptions;
using Constellation.Application.Features.Auth.Command;
using Constellation.Application.Features.Auth.Queries;
using Constellation.Application.Features.Portal.School.Home.Queries;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.Features.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;

public class LoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMediator _mediator;
    private readonly ILogger<IAuthService> _logger;
    private readonly IActiveDirectoryActionsService _adService;

    public LoginModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager,
        IMediator mediator, ILogger<IAuthService> logger, IActiveDirectoryActionsService adService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _mediator = mediator;
        _logger = logger;
        _adService = adService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ReturnUrl { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [RegularExpression(@"^\w+([-+.']\w+)*@det.nsw.edu.au$", ErrorMessage = "Invalid Email.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            _logger.LogInformation($"Starting Login Attempt by {Input.Email}");

            // Check valid domain login
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

            // Check email address is valid user
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
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
                        user = await _userManager.FindByEmailAsync(Input.Email);

                        _logger.LogWarning(" - User created for email {user}", Input.Email);
                    }
                    else
                    {
                        _logger.LogWarning(" - No valid linked school found for non-existing user {user}", Input.Email);
                        throw new PortalAppAuthenticationException("Could not find valid Partner School to link new user with.");
                    }
                }
                else
                {
                    _logger.LogWarning(" - Found inactive user for email {user}", Input.Email);
                    // Recreate user account based on SchoolContact entry
                    await _mediator.Send(new RepairSchoolContactUserCommand { SchoolContactId = dbContact.Id });
                    user = await _userManager.FindByEmailAsync(Input.Email);

                    _logger.LogWarning(" - Inactive user reactivated for email {user}", Input.Email);
                }
            }

            // Is user an admin?
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains(AuthRoles.Admin))
            {
                var schools = await _mediator.Send(new GetAllPartnerSchoolCodesQuery());
                var claimList = new List<Claim>();
                foreach (var entry in schools)
                {
                    claimList.Add(new Claim("Schools", entry));
                }

                _logger.LogInformation($" - Admin access granted. Logging in with system wide access.");
                await _signInManager.SignInWithClaimsAsync(user, false, claimList);

                _logger.LogInformation(" - Login succeeded for {user}", Input.Email);

                if (string.IsNullOrWhiteSpace(returnUrl))
                    return LocalRedirect(returnUrl!);
                else
                    return LocalRedirect("");
            }

            // Build/rebuild schools claim
            await _mediator.Send(new UpdateUserSchoolsClaimCommand { EmailAddress = Input.Email });

            await _signInManager.SignInAsync(user, false);

            _logger.LogInformation(" - Login succeeded for {user}", Input.Email);

            if (string.IsNullOrWhiteSpace(returnUrl))
                return LocalRedirect(returnUrl!);
            else
                return LocalRedirect("");
        }

        return Page();
    }
}

namespace Constellation.Portal.Schools.Server.Areas.Identity.Pages.Account;

using Application.SchoolContacts.CreateContactFromActiveDirectory;
using Application.Users.RepairSchoolContactUser;
using Constellation.Application.Exceptions;
using Constellation.Application.Features.Auth.Command;
using Constellation.Application.Features.Auth.Queries;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models.SchoolContacts;
using Core.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.AccountManagement;

public class LoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMediator _mediator;
    private readonly Serilog.ILogger _logger;
    private readonly IActiveDirectoryActionsService _adService;

    public LoginModel(
        SignInManager<AppUser> signInManager, 
        UserManager<AppUser> userManager,
        IMediator mediator, 
        Serilog.ILogger logger, 
        IActiveDirectoryActionsService adService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _mediator = mediator;
        _logger = logger.ForContext<IAuthService>();
        _adService = adService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ReturnUrl { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        //[EmailAddress]
        [RegularExpression(@"^(?:~+.*$)|\w+(?:[-+.']\w+)*@det.nsw.edu.au$", ErrorMessage = "Invalid Email.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public async Task OnGetAsync(string? returnUrl = null) =>
        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid) return Page();
        
        _logger
            .Information("Starting Login Attempt by {Email}", Input.Email);

#if DEBUG
        if (Input.Email.StartsWith('~'))
        {
            Input.Email = Input.Email.Replace("~", "");
                
            AppUser? bypassUser = await _userManager.FindByEmailAsync(Input.Email);

            await _signInManager.SignInAsync(bypassUser, false);

            _logger
                .Information(" - BYPASS Login succeeded for {user}", Input.Email);

            return LocalRedirect(returnUrl!);
        }
#endif

        // Check valid domain login
        _logger
            .Information(" - Attempting domain login by {user}", Input.Email);

        PrincipalContext context = new PrincipalContext(ContextType.Domain, "DETNSW.WIN");
        bool success = context.ValidateCredentials(Input.Email, Input.Password);

        if (!success)
        {
            _logger
                .Warning(" - Domain login failed for {user}", Input.Email);

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        _logger
            .Information(" - Domain login succeeded for {user}", Input.Email);

        // Check email address is valid user
        AppUser? user = await _userManager.FindByEmailAsync(Input.Email);
        if (user is null)
        {
            _logger
                .Warning(" - No user found for email {user}", Input.Email);

            SchoolContact? dbContact = await _mediator.Send(new GetSchoolContactByEmailAddressQuery { EmailAddress = Input.Email });

            if (dbContact == null)
            {
                _logger
                    .Warning(" - No Contact Record found for email {user}", Input.Email);

                // Check that user is linked to valid school first.
                List<string>? adSchools = await _adService.GetLinkedSchoolsFromAD(Input.Email);

                if (adSchools is { Count: > 0 })
                {
                    _logger
                        .Warning(" - Found valid linked school in AD Records for user {user}", Input.Email);

                    // Send to registration
                    await _mediator.Send(new CreateContactFromActiveDirectoryCommand(Input.Email));
                    user = await _userManager.FindByEmailAsync(Input.Email);

                    _logger
                        .Warning(" - User created for email {user}", Input.Email);
                }
                else
                {
                    _logger
                        .Warning(" - No valid linked school found for non-existing user {user}", Input.Email);
                    throw new PortalAppAuthenticationException("Could not find valid Partner School to link new user with.");
                }
            }
            else
            {
                _logger
                    .Warning(" - Found inactive user for email {user}", Input.Email);

                // Recreate user account based on SchoolContact entry
                await _mediator.Send(new RepairSchoolContactUserCommand(dbContact.Id));
                user = await _userManager.FindByEmailAsync(Input.Email);

                _logger
                    .Warning(" - Inactive user reactivated for email {user}", Input.Email);
            }
        }

        // Is user an admin?
        IList<string>? roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains(AuthRoles.Admin))
        {
            //ICollection<string>? schools = await _mediator.Send(new GetAllPartnerSchoolCodesQuery());
            //List<Claim> claimList = new List<Claim>();

            //foreach (string entry in schools)
            //{
            //    claimList.Add(new Claim("Schools", entry));
            //}

            _logger
                .Information(" - Admin access granted. Logging in with system wide access.");

            await _signInManager.SignInAsync(user, false);

            _logger
                .Information(" - Login succeeded for {user}", Input.Email);

            return LocalRedirect(returnUrl!);
        }

        // Build/rebuild schools claim
        await _mediator.Send(new UpdateUserSchoolsClaimCommand { EmailAddress = Input.Email });

        await _signInManager.SignInAsync(user, false);

        _logger
            .Information(" - Login succeeded for {user}", Input.Email);

        user.LastLoggedIn = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return LocalRedirect(returnUrl!);
    }
}

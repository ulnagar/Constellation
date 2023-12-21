namespace Constellation.Presentation.Server.Areas.Admin.Pages;

using Constellation.Application.Features.Auth.Queries;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly Serilog.ILogger _logger;
    private readonly IMediator _mediator;
    private readonly SignInManager<AppUser> _signInManager;
    
    public LoginModel(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        Serilog.ILogger logger,
        IMediator mediator)
    {
        _userManager = userManager;
        _logger = logger.ForContext<IAuthService>();
        _mediator = mediator;
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [RegularExpression(@"^(?:~+.*$)|\w+(?:[-+.']\w+)*@det.nsw.edu.au$", ErrorMessage = "Invalid Email.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

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
        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid) return Page();

        // Allow bypass of authentication when debugging
        bool localAuth = Input.Email.Contains("~");
        Input.Email = Input.Email.Replace("~", "");

        _logger.Information("Starting Login Attempt by {Email}", Input.Email);

        AppUser user = await _userManager.FindByEmailAsync(Input.Email);

        if (user == null)
        {
            _logger.Warning(" - No user found for email {Email}", Input.Email);

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        } 

        _logger.Information(" - Found user {user} for email {email}", user.Id, Input.Email);

        bool isAdmin = await _userManager.IsInRoleAsync(user, AuthRoles.Admin);

        // If user is admin, bypass Staff Member check
        if (!isAdmin)
        {
            bool isStaffMember = await _mediator.Send(new IsUserASchoolStaffMemberQuery { EmailAddress = Input.Email });

            // Check both the staff list for matching email, and the user object for correct flag
            if (!isStaffMember && !user.IsStaffMember)
            {
                _logger.Information(" - User is not a staff member - redirecting to Schools Portal");
                return RedirectToPage("PortalRedirect");
            }
        }

        LoginResult result = new();

        if (!localAuth)
        {
            _logger.Information(" - Attempting domain login by {Email}", Input.Email);

#pragma warning disable CA1416 // Validate platform compatibility
            PrincipalContext context = new(ContextType.Domain, "DETNSW.WIN");
            bool success = context.ValidateCredentials(Input.Email, Input.Password);
#pragma warning restore CA1416 // Validate platform compatibility

            if (!success)
            {
                _logger.Warning(" - Domain login failed for {Email}", Input.Email);

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            _logger.Information(" - Domain login succeeded for {Email}", Input.Email);

            await _signInManager.SignInAsync(user, false);
            result.Succeeded = true;
        }
        else
        {
            _logger.Information(" - Attempting local login by {Email}", Input.Email);
#if DEBUG
            _logger.Information(" - DEBUG code found. Bypass login check.");
            await _signInManager.SignInAsync(user, false);
            result.Succeeded = true;
#else
            var passwordResult = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            
            if (!passwordResult.Succeeded)
            {
                _logger.Warning(" - Local login failed for {Email}", Input.Email);
            }
            else
            {
                result.Succeeded = passwordResult.Succeeded;
                _logger.Information(" - Local login suceeded for {Email}", Input.Email);
            }
#endif
        }

        if (result.Succeeded)
        {
            user.LastLoggedIn = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return LocalRedirect(returnUrl);
        }
        
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }
}

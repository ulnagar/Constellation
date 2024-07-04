namespace Constellation.Presentation.Server.Areas.Admin.Pages;

using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ILogger _logger;

    public LoginModel(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ILogger logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger.ForContext<IAuthService>();
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [RegularExpression(@"^(?:~+.*$)|\w+(?:[-+.']\w+)*@det.nsw.edu.au$", ErrorMessage = "Invalid Email.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public enum LoginType
    {
        Local,
        Debug
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
        LoginType loginType = LoginType.Local;

        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid) return Page();

        // Allow bypass of authentication when debugging
        loginType = Input.Email.StartsWith('~') ? LoginType.Debug : LoginType.Local;
        Input.Email = Input.Email.Replace("~", "");
        
        _logger.Information("Starting Login Attempt by {Email}", Input.Email);
        AppUser user = await _userManager.FindByEmailAsync(Input.Email);

        if (user is null)
        {
            _logger.Warning(" - No user found for email {Email}", Input.Email);

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        } 

        _logger.Information(" - Found user {user} for email {email}", user.Id, Input.Email);

        bool result = new();

        if (loginType == LoginType.Local)
        {
            _logger.Information(" - Attempting domain login by {Email}", Input.Email);

            PrincipalContext context = new(ContextType.Domain, "DETNSW.WIN");
            result = context.ValidateCredentials(Input.Email, Input.Password);
            context.Dispose();

            if (!result)
            {
                _logger.Warning(" - Domain login failed for {Email}", Input.Email);

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            _logger.Information(" - Domain login succeeded for {Email}", Input.Email);

            await _signInManager.SignInAsync(user, false);
        }

#if DEBUG
        if (loginType == LoginType.Debug)
        {
            _logger.Information(" - DEBUG code found. Bypass login check.");
            await _signInManager.SignInAsync(user, false);

            result = true;
        }
#endif

        if (result)
        {
            user.LastLoggedIn = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return LocalRedirect(returnUrl);
        }
        
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }
}

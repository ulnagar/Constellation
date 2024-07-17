namespace Constellation.Presentation.Server.Areas.Admin.Pages;

using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Core.Shared;
using Core.ValueObjects;
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
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public LoginModel(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IEmailService emailService,
        ILogger logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _logger = logger.ForContext<IAuthService>();
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        [EmailAddress]
        //[RegularExpression(@"^(?:~+.*$)|\w+(?:[-+.']\w+)*@det.nsw.edu.au$", ErrorMessage = "Invalid Email.")]
        [RegularExpression(@"^(?:~+.*$)|\w+(?:[-+.']\w+)*@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$", ErrorMessage = "Invalid Email.")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string? Password { get; set; } = string.Empty;
    }

    internal enum LoginType
    {
        Local,
        Domain,
        MagicLink,
        Debug
    }

    public enum LoginStatus
    {
        WaitingUserInput,
        WaitingPasswordInput,
        EmailSent,
        InvalidUsername,
        TokenInvalid
    }

    public LoginStatus Status { get; set; } = LoginStatus.WaitingUserInput;

    public async Task OnGet(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        Status = LoginStatus.WaitingUserInput;

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        if (!ModelState.IsValid)
            return Page();

        returnUrl ??= Url.Content("~/");

        LoginType loginType = GetLoginParameters();

        _logger.Information("Starting Login Attempt by {Email}", Input.Email);
        AppUser user = await _userManager.FindByEmailAsync(Input.Email);

        if (user is null)
        {
            _logger.Warning(" - No user found for email {Email}", Input.Email);

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");

            Status = LoginStatus.InvalidUsername;

            return Page();
        } 

        _logger.Information(" - Found user {user} for email {email}", user.Id, Input.Email);

        bool result = new();

        if (loginType == LoginType.Domain)
        {
            Status = LoginStatus.WaitingPasswordInput;

            return Page();
        }

#if DEBUG
        if (loginType == LoginType.Debug)
        {
            _logger.Information(" - DEBUG code found. Bypass login check.");
            await _signInManager.SignInAsync(user, false);

            return LocalRedirect(returnUrl);
        }
#endif

        if (loginType == LoginType.MagicLink)
        {
            string token = await _userManager.GenerateUserTokenAsync(user, "PasswordlessLoginProvider", "passwordless-auth");

            // Create login url with embedded token
            string url = Url.Page("Login", "Passwordless", new { token, userId = user.Id.ToString() }, Request.Scheme);

            // Email login url to user
            MagicLinkEmail notification = new()
            {
                Link = url,
                Name = user.DisplayName
            };

            Result<EmailRecipient> recipient = EmailRecipient.Create(user.DisplayName, user.Email);

            if (recipient.IsFailure)
            {
                _logger.Warning(" - Could not generate email recipient for user {@user}", user);
                Status = LoginStatus.InvalidUsername;

                return Page();
            }

            notification.Recipients.Add(recipient.Value);

            await _emailService.SendMagicLinkLoginEmail(notification);

            _logger.Information(" - Magic login link sent to user {user}", Input.Email);

            // Present user with confirmation that email has been sent
            Status = LoginStatus.EmailSent;

            return Page();
        }
        
        ModelState.AddModelError(string.Empty, "Invalid user account.");
        Status = LoginStatus.InvalidUsername;

        return Page();
    }

    private LoginType GetLoginParameters()
    {
        LoginType loginType = LoginType.Local;
        
        switch (Input.Email)
        {
            case not null when Input.Email.StartsWith('~'):
                loginType = LoginType.Debug;
                Input.Email = Input.Email.Replace("~", "");
                break;
            case not null when Input.Email.Contains("@det.nsw.edu.au"):
            case not null when Input.Email.Contains("@education.nsw.gov.au"):
                loginType = LoginType.Domain;
                break;
            default:
                loginType = LoginType.MagicLink;
                break;
        }

        return loginType;
    }

    public async Task<IActionResult> OnPostPasswordLogin(string returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(Input.Password))
        {
            ModelState.TryAddModelError(nameof(Input.Password), "You must specify a password!");

            Status = LoginStatus.WaitingPasswordInput;
        }

        if (!ModelState.IsValid) return Page();

        LoginType loginType = GetLoginParameters();

        returnUrl ??= Url.Content("~/");
        
        _logger.Information("Continuing Login Attempt by {Email}", Input.Email);
        AppUser user = await _userManager.FindByEmailAsync(Input.Email);

        _logger.Information(" - Found user {user} for email {email}", user.Id, Input.Email);

        if (loginType == LoginType.Domain)
        {
            _logger.Information(" - Attempting domain login by {Email}", Input.Email);

            PrincipalContext context = new(ContextType.Domain, "DETNSW.WIN");
            bool result = context.ValidateCredentials(Input.Email, Input.Password);
            context.Dispose();

            if (!result)
            {
                _logger.Warning(" - Domain login failed for {Email}", Input.Email);

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");

                Status = LoginStatus.WaitingPasswordInput;

                return Page();
            }

            _logger.Information(" - Domain login succeeded for {Email}", Input.Email);

            await _signInManager.SignInAsync(user, false);

            user.LastLoggedIn = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return LocalRedirect(returnUrl);
        }
        
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");

        Status = LoginStatus.InvalidUsername;

        return Page();
    }

    public async Task<IActionResult> OnGetPasswordless(string token, string userId)
    {
        _logger.Information("Continuing Login Attempt by {user}", userId);

        // Get user entry from database
        AppUser user = await _userManager.FindByIdAsync(userId);

        _logger.Information("Found user {user} with Id {id}", user.Email, userId);

        // Verify login token in url
        bool isValid = await _userManager.VerifyUserTokenAsync(user, "PasswordlessLoginProvider", "passwordless-auth", token);

        if (!isValid)
        {
            _logger.Warning(" - Token invalid for {user}", user.Email);

            Status = LoginStatus.TokenInvalid;

            return Page();
        }
        
        // Log user in
        await _signInManager.SignInAsync(user, false);

        _logger.Information(" - Login succeeded for {user}", user.Email);

        user.LastLoggedIn = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Redirect to home page
        return RedirectToPage("/Dashboard", new { area = "Parents" });
    }
}

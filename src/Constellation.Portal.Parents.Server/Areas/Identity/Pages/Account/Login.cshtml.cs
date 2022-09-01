namespace Constellation.Portal.Parents.Server.Areas.Identity.Pages.Account;

using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Features.Auth.Queries;
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
    private readonly IEmailService _emailService;
    private readonly IMediator _mediator;
    private readonly ILogger<IAuthService> _logger;

    internal enum LoginStatus
    {
        WaitingUserInput,
        EmailSent,
        InvalidUsername,
        TokenInvalid
    }

    public LoginModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager,
        IEmailService emailService, IMediator mediator, ILogger<IAuthService> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _emailService = emailService;
        _mediator = mediator;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ReturnUrl { get; set; } = String.Empty;

    internal LoginStatus Status { get; set; } = LoginStatus.WaitingUserInput;

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = String.Empty;
    }

    /// <summary>
    /// First page visit. Request email address from user.
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    public async Task OnGetAsync(string? returnUrl = null)
    {
        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
    }

    // Provide login experience using Magic Link (as per https://gist.github.com/ebicoglu/04cedc99d0365f4d20a6233cca69cf5b)
    /// <summary>
    /// User has supplied an email address. Verify and send link.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            _logger.LogInformation($"Starting Login Attempt by {Input.Email}");

            // Check email address is valid user
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                _logger.LogWarning(" - No user found for email {user}", Input.Email);

                Status = LoginStatus.InvalidUsername;
                return Page();
            }

            // Confirm user is a parent
            var isValid = await _mediator.Send(new IsUserAParentQuery { EmailAddress = Input.Email });
            if (!isValid)
            {
                _logger.LogWarning(" - User {user} has not corresponding parent record", Input.Email);

                Status = LoginStatus.InvalidUsername;
                return Page();
            }

            // Get token from provider
            var token = await _userManager.GenerateUserTokenAsync(user, "PasswordlessLoginProvider", "passwordless-auth");

            // Create login url with embedded token
            var url = Url.Page("Login", "Passwordless", new { token, userId = user.Id.ToString() }, Request.Scheme);

            // Email login url to user
            var notification = new MagicLinkEmail
            {
                Link = url,
                Name = user.DisplayName
            };

            notification.Recipients.Add(new EmailBaseClass.Recipient { Name = user.DisplayName, Email = user.Email });

            await _emailService.SendMagicLinkLoginEmail(notification);

            _logger.LogInformation(" - Magic login link sent to user {user}", Input.Email);

            // Present user with confirmation that email has been sent
            Status = LoginStatus.EmailSent;

            return Page();
        }

        // There was a problem with the data returned. Show an error.
        Status = LoginStatus.InvalidUsername;

        return Page();
    }

    /// <summary>
    /// A Passwordless Login token has been supplied. Process login.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnGetPasswordlessAsync(string token, string userId)
    {
        _logger.LogInformation("Continuing Login Attempt by {user}", userId);

        // Get user entry from database
        var user = await _userManager.FindByIdAsync(userId);

        // Verify login token in url
        var isValid = await _userManager.VerifyUserTokenAsync(user, "PasswordlessLoginProvider", "passwordless-auth", token);
        
        if (!isValid)
        {
            _logger.LogWarning(" - Token invalid for {user}", Input.Email);

            Status = LoginStatus.TokenInvalid;

            return Page();
        }

        // Log out all other sessions
        await _userManager.UpdateSecurityStampAsync(user);

        // Log user in
        await _signInManager.SignInAsync(user, false);

        _logger.LogInformation(" - Login succeeded for {user}", user.Email);

        // Redirect to home page
        return LocalRedirect(Url.Content("~/"));
    }
}

namespace Constellation.Portal.Parents.Server.Areas.Identity.Pages.Account;

using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Core.Abstractions;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using System.ComponentModel.DataAnnotations;

public class LoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IMediator _mediator;
    private readonly IFamilyRepository _familyRepository;
    private readonly ILogger _logger;

    internal enum LoginStatus
    {
        WaitingUserInput,
        EmailSent,
        InvalidUsername,
        TokenInvalid
    }

    public LoginModel(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        IEmailService emailService,
        IMediator mediator,
        IFamilyRepository familyRepository,
        ILogger logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _emailService = emailService;
        _mediator = mediator;
        _familyRepository = familyRepository;
        _logger = logger.ForContext<IAuthService>();
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
            _logger.Information($"Starting Login Attempt by {Input.Email}");

            // Check email address is valid user
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                _logger.Warning(" - No user found for email {user}", Input.Email);

                Status = LoginStatus.InvalidUsername;
                return Page();
            }

            // Confirm user is a parent
            var isValid = await _familyRepository.DoesEmailBelongToParentOrFamily(Input.Email);
            if (!isValid)
            {
                _logger.Warning(" - User {user} has not corresponding parent record", Input.Email);

                Status = LoginStatus.InvalidUsername;
                return Page();
            }

#if DEBUG
            await _signInManager.SignInAsync(user, false);

            _logger.Information(" - Login succeeded for {user}", user.Email);

            user.LastLoggedIn = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Redirect to home page
            return LocalRedirect(Url.Content("~/"));
#endif

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
        _logger.Information("Continuing Login Attempt by {user}", userId);

        // Get user entry from database
        var user = await _userManager.FindByIdAsync(userId);

        _logger.Information("Found user {user} with Id {id}", user.Email, userId);

        // Verify login token in url
        var isValid = await _userManager.VerifyUserTokenAsync(user, "PasswordlessLoginProvider", "passwordless-auth", token);
        
        if (!isValid)
        {
            _logger.Warning(" - Token invalid for {user}", user.Email);

            Status = LoginStatus.TokenInvalid;

            return Page();
        }

        // Log out all other sessions
        //await _userManager.UpdateSecurityStampAsync(user);

        // Log user in
        await _signInManager.SignInAsync(user, false);

        _logger.Information(" - Login succeeded for {user}", user.Email);

        user.LastLoggedIn = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Redirect to home page
        return LocalRedirect(Url.Content("~/"));
    }
}

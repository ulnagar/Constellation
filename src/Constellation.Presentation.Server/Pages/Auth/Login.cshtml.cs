namespace Constellation.Presentation.Server.Pages.Auth;

using Application.Domains.AppSettings.Models;
using Application.Domains.Auth.Queries.GetParentUserFromMobileNumber;
using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Core.Models.Auth;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Presentation.Shared.Extensions;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly ISender _mediator;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IAppSettingsService _appSettings;
    private readonly IEmailService _emailService;
    private readonly ISMSService _smsService;
    private readonly ILogger _logger;

    public LoginModel(
        ISender mediator,
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IAppSettingsService appSettings,
        IEmailService emailService,
        ISMSService smsService,
        ILogger logger)
    {
        _mediator = mediator;
        _userManager = userManager;
        _signInManager = signInManager;
        _appSettings = appSettings;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger
            .ForContext<LoginModel>()
            .ForStaffPortal();
    }

    [BindProperty]
    public InputModel Input { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Manual { get; set; }
    
    public bool LoginEnabled { get; set; } = true;
    public bool SSOEnabled { get; set; }

    public class InputModel
    {
        [Required]
        //[EmailAddress]
        //[RegularExpression(@"^(?:~+.*$)|\w+(?:[-+.']\w+)*@det.nsw.edu.au$", ErrorMessage = "Invalid Email.")]
        [RegularExpression(@"^(?:~+.*$)|^(?:!+.*$)|^(?:[0-9]{10}$)|^\w+(?:[-+.']\w+)*@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$", ErrorMessage = "Invalid Email.")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string? Password { get; set; } = string.Empty;
    }

    internal enum LoginType
    {
        Local,
        Domain,
        MagicLink,
        Sms,
        SSO,
        Debug
    }

    public enum LoginStatus
    {
        WaitingUserInput,
        WaitingPasswordInput,
        WaitingTokenInput,
        EmailSent,
        InvalidUsername,
        TokenInvalid
    }

    public LoginStatus Status { get; set; } = LoginStatus.WaitingUserInput;

    private async Task PreparePage()
    {
        AuthenticationConfiguration? configuration = await _appSettings.Authentication();

        if (configuration is not null)
        {
            LoginEnabled = configuration.LoginEnabled;
            SSOEnabled = configuration.SSOEnabled;
        }

        if (!LoginEnabled && Manual)
            LoginEnabled = true;
    }

    public async Task<IActionResult> OnGet()
    {
        string? sessionUser = HttpContext.Session.GetString("KnownUser");
        string? cookieUser = Request.Cookies[".Constellation.KnownUser"];

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync();

        await PreparePage();

        if (SSOEnabled && !Manual && !string.IsNullOrWhiteSpace(sessionUser))
            // This browser session already had a successful SSO login - 
            // treat this as a timeout, not a fresh attempt.
            return ChallengeSingleSignOn(sessionUser);
        
        if (!string.IsNullOrWhiteSpace(sessionUser))
            Input.Email = sessionUser;
        else if (!string.IsNullOrWhiteSpace(cookieUser))
            Input.Email = cookieUser;

        Status = LoginStatus.WaitingUserInput;
        return Page();
    }

    private IActionResult ChallengeSingleSignOn(string? loginHint)
    {
        AuthenticationProperties props = new();
        if (!string.IsNullOrWhiteSpace(loginHint))
            props.Items["login_hint"] = loginHint;

        return Challenge(props, OpenIdConnectDefaults.AuthenticationScheme);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await PreparePage();

        if (!ModelState.IsValid)
            return Page();

        LoginType loginType = GetLoginParameters();

        _logger.Information("Starting Login Attempt by {Email}", Input.Email);

        if (loginType == LoginType.Sms)
        {
            Result<PhoneNumber> phoneNumber = PhoneNumber.Create(Input.Email);

            if (phoneNumber.IsFailure)
            {
                ModelState.AddModelError(string.Empty, "Invalid user account.");
                Status = LoginStatus.InvalidUsername;

                return Page();
            }

            Result<AppUser> parent = await _mediator.Send(new GetParentUserFromMobileNumberQuery(phoneNumber.Value));

            if (parent.IsFailure)
            {
                _logger.Warning(" - No user found for mobile {Mobile}", Input.Email);

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");

                Status = LoginStatus.InvalidUsername;

                return Page();
            }
            
            string token = await _userManager.GenerateUserTokenAsync(parent.Value, "PasswordlessLoginProvider", "passwordless-auth");
            
            await _smsService.SendLoginToken(token, phoneNumber.Value);

            _logger.Information(" - Login token sent to user {user}", Input.Email);

            // Present user with confirmation that email has been sent
            Status = LoginStatus.WaitingTokenInput;

            return Page();
        }

        AppUser? user = await _userManager.FindByEmailAsync(Input.Email);

        if (user is null)
        {
            _logger.Warning(" - No user found for email {Email}", Input.Email);

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");

            Status = LoginStatus.InvalidUsername;

            return Page();
        } 

        _logger.Information(" - Found user {user} for email {email}", user.Id, Input.Email);

        user.AddLogin(DateTime.UtcNow, Core.Models.Auth.Enums.LoginStatus.Started);

        await _userManager.UpdateAsync(user);

        if (loginType == LoginType.SSO)
            return ChallengeSingleSignOn(Input.Email);

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

            return LocalRedirect("/");
        }
#endif

        if (loginType == LoginType.MagicLink)
        {
            string token = await _userManager.GenerateUserTokenAsync(user, "PasswordlessLoginProvider", "passwordless-auth");

            // Create login url with embedded token
            string? url = Url.Page("Login", "Passwordless", new { token, userId = user.Id.ToString() }, Request.Scheme);

            // Email login url to user
            MagicLinkEmail notification = new()
            {
                Link = url,
                Name = user.Name.DisplayName
            };

            Result<EmailRecipient> recipient = EmailRecipient.Create(user.Name.DisplayName, user.Email);

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
            case not null when Input.Email.StartsWith('!'):
                loginType = LoginType.SSO;
                Input.Email = Input.Email.Replace("!", "", StringComparison.InvariantCultureIgnoreCase);
                break;
            case not null when Input.Email.StartsWith('~'):
                loginType = LoginType.Debug;
                Input.Email = Input.Email.Replace("~", "", StringComparison.InvariantCultureIgnoreCase);
                break;
            case not null when Input.Email.Contains("@det.nsw.edu.au", StringComparison.InvariantCultureIgnoreCase):
            case not null when Input.Email.Contains("@education.nsw.gov.au", StringComparison.InvariantCultureIgnoreCase):
                loginType = SSOEnabled ? LoginType.SSO : LoginType.Domain;
                break;
            case not null when Input.Email.All(Char.IsDigit):
                loginType = LoginType.Sms;
                break;
            default:
                loginType = LoginType.MagicLink;
                break;
        }

        return loginType;
    }

    public async Task<IActionResult> OnPostPasswordLogin()
    {
        await PreparePage();

        if (string.IsNullOrWhiteSpace(Input.Password))
        {
            ModelState.TryAddModelError(nameof(Input.Password), "You must specify a password!");

            Status = LoginStatus.WaitingPasswordInput;
        }

        if (!ModelState.IsValid) return Page();

        LoginType loginType = GetLoginParameters();

        _logger.Information("Continuing Login Attempt by {Email}", Input.Email);
        AppUser? user = await _userManager.FindByEmailAsync(Input.Email);

        if (user is null)
            return Page();

        _logger.Information(" - Found user {user} for email {email}", user.Id, Input.Email);

        if (loginType == LoginType.Domain)
        {
            _logger.Information(" - Attempting domain login by {Email}", Input.Email);

            PrincipalContext context = new(ContextType.Domain, "DETNSW.WIN");

            bool result = Input.Email.Contains("@education.nsw.gov.au", StringComparison.InvariantCultureIgnoreCase)
                ? context.ValidateCredentials(Input.Email.Replace("education.nsw.gov.au", "detnsw", StringComparison.InvariantCultureIgnoreCase), Input.Password)
                : context.ValidateCredentials(Input.Email, Input.Password);

            context.Dispose();

            if (!result)
            {
                _logger.Warning(" - Domain login failed for {Email}", Input.Email);

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");

                Status = LoginStatus.WaitingPasswordInput;

                user.AddLogin(DateTime.UtcNow, Core.Models.Auth.Enums.LoginStatus.Failed);

                await _userManager.UpdateAsync(user);

                return Page();
            }

            _logger.Information(" - Domain login succeeded for {Email}", Input.Email);

            await _signInManager.SignInAsync(user, false);

            user.AddLogin(DateTime.UtcNow, Core.Models.Auth.Enums.LoginStatus.Success);
            await _userManager.UpdateAsync(user);

            return LocalRedirect("/Index");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");

        Status = LoginStatus.InvalidUsername;

        user.AddLogin(DateTime.UtcNow, Core.Models.Auth.Enums.LoginStatus.Failed);

        await _userManager.UpdateAsync(user);

        return Page();
    }

    public async Task<IActionResult> OnGetPasswordless(string token, string userId)
    {
        await PreparePage();

        _logger.Information("Continuing Login Attempt by {user}", userId);

        // Get user entry from database
        AppUser? user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Page();

        _logger.Information("Found user {user} with Id {id}", user.Email, userId);

        // Verify login token in url
        bool isValid = await _userManager.VerifyUserTokenAsync(user, "PasswordlessLoginProvider", "passwordless-auth", token);

        if (!isValid)
        {
            _logger.Warning(" - Token invalid for {user}", user.Email);

            Status = LoginStatus.TokenInvalid;

            user.AddLogin(DateTime.UtcNow, Core.Models.Auth.Enums.LoginStatus.Failed);

            await _userManager.UpdateAsync(user);

            return Page();
        }
        
        // Log user in
        await _signInManager.SignInAsync(user, false);

        _logger.Information(" - Login succeeded for {user}", user.Email);
        
        user.AddLogin(DateTime.UtcNow, Core.Models.Auth.Enums.LoginStatus.Success);

        await _userManager.UpdateAsync(user);

        // Redirect to home page
        return RedirectToPage("/Dashboard", new { area = "Parents" });
    }

    public async Task<IActionResult> OnPostTokenLogin()
    {
        await PreparePage();

        if (string.IsNullOrWhiteSpace(Input.Password))
        {
            ModelState.TryAddModelError(nameof(Input.Password), "You must specify a token!");

            Status = LoginStatus.WaitingTokenInput;
        }

        if (!ModelState.IsValid) return Page();

        LoginType loginType = GetLoginParameters();

        Result<PhoneNumber> phoneNumber = PhoneNumber.Create(Input.Email);

        if (phoneNumber.IsFailure)
        {
            ModelState.AddModelError(string.Empty, "Invalid user account.");
            Status = LoginStatus.InvalidUsername;

            return Page();
        }

        Result<AppUser> parent = await _mediator.Send(new GetParentUserFromMobileNumberQuery(phoneNumber.Value));

        if (parent.IsFailure)
        {
            _logger.Warning(" - No user found for mobile {Mobile}", Input.Email);

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");

            Status = LoginStatus.InvalidUsername;

            return Page();
        }

        _logger.Information("Continuing Login Attempt by {Email}", parent.Value.Email);

        _logger.Information(" - Found user {user} for email {email}", parent.Value.Id, parent.Value.Email);

        if (loginType == LoginType.Sms)
        {
            _logger.Information(" - Attempting SMS login by {Email}", parent.Value.Email);
            
            bool isValid = await _userManager.VerifyUserTokenAsync(parent.Value, "PasswordlessLoginProvider", "passwordless-auth", Input.Password!);

            if (!isValid)
            {
                _logger.Warning(" - Token invalid for {user}", parent.Value.Email);

                Status = LoginStatus.TokenInvalid;
                
                parent.Value.AddLogin(DateTime.UtcNow, Core.Models.Auth.Enums.LoginStatus.Failed);
                await _userManager.UpdateAsync(parent.Value);

                return Page();
            }

            // Log user in
            await _signInManager.SignInAsync(parent.Value, false);

            _logger.Information(" - Login succeeded for {user}", parent.Value.Email);

            parent.Value.AddLogin(DateTime.UtcNow, Core.Models.Auth.Enums.LoginStatus.Success);
            await _userManager.UpdateAsync(parent.Value);

            // Redirect to home page
            return RedirectToPage("/Dashboard", new { area = "Parents" });
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");

        Status = LoginStatus.InvalidUsername;

        parent.Value.AddLogin(DateTime.UtcNow, Core.Models.Auth.Enums.LoginStatus.Failed);
        await _userManager.UpdateAsync(parent.Value);

        return Page();
    }
}

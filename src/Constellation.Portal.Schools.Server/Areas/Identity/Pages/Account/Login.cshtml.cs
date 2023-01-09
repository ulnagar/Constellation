namespace Constellation.Portal.Schools.Server.Areas.Identity.Pages.Account;

using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Features.Auth.Queries;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.Features.Auth.Queries;
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
    private readonly IMediator _mediator;
    private readonly ILogger<IAuthService> _logger;

    public LoginModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager,
        IMediator mediator, ILogger<IAuthService> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _mediator = mediator;
        _logger = logger;
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

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            _logger.LogInformation($"Starting Login Attempt by {Input.Email}");

            // Check email address is valid user
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                _logger.LogWarning(" - No user found for email {user}", Input.Email);

                return Page();
            }

            _logger.LogInformation("Found user {user} with Id {id}", user.Email, user.Id);

            // Log out all other sessions
            await _userManager.UpdateSecurityStampAsync(user);

            // Log user in
            await _signInManager.SignInAsync(user, false);

            _logger.LogInformation(" - Login succeeded for {user}", user.Email);

            // Redirect to home page
            return LocalRedirect(Url.Content("~/"));
        }

        return Page();
    }
}

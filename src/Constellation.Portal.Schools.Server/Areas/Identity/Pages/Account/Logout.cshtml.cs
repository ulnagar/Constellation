#nullable disable
namespace Constellation.Portal.Schools.Server.Areas.Identity.Pages.Account;

using Constellation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class LogoutModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly Serilog.ILogger _logger;

    public LogoutModel(SignInManager<IdentityUser> signInManager, Serilog.ILogger logger)
    {
        _signInManager = signInManager;
        _logger = logger.ForContext<IAuthService>();
    }

    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        var username = User.Identity.Name;

        await _signInManager.SignOutAsync();
        _logger.Information("User {user} logged out.", username);
        if (returnUrl != null)
        {
            return LocalRedirect(returnUrl);
        }
        else
        {
            // This needs to be a redirect so that the browser performs a new
            // request and the identity for the user gets updated.
            return RedirectToPage();
        }
    }
}

namespace Constellation.Presentation.Server.Pages.Auth;

using BaseModels;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[AllowAnonymous]
public class LogoutSSOModel : BasePageModel
{
    private readonly SignInManager<AppUser> _signInManager;

    public LogoutSSOModel(
        SignInManager<AppUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnGet()
    {
        await _signInManager.SignOutAsync();

        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync();
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

        return RedirectToPage("/Index", new { area = "" });

    }
}
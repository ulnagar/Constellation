namespace Constellation.Presentation.Server.Pages.Auth;

using BaseModels;
using Core.Models.Auth;
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
        Response.Cookies.Delete(".Constellation.KnownUser");
        HttpContext.Session.Clear();

        return RedirectToPage("/Index", new { area = "" });
    }
}
namespace Constellation.Presentation.Server.Pages.Auth;

using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[AllowAnonymous]
public class LogoutModel : BasePageModel
{
    private readonly SignInManager<AppUser> _signInManager;

    public LogoutModel(SignInManager<AppUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task OnGet()
    {
        await _signInManager.SignOutAsync();

        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync();
        //await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

        //return RedirectToPage("/Index", new { area = "" });
    }

    //public async Task<IActionResult> OnGet()
    //{
    //    await _signInManager.SignOutAsync();

    //    HttpContext.Session.Clear();
    //    await HttpContext.SignOutAsync();
    //    //await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
         
    //    return RedirectToPage("/Index", new { area = ""});
    //}

    public async Task<IActionResult> OnPost()
    {
        await _signInManager.SignOutAsync();

        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync();
        //await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

        return RedirectToPage("/Index", new { area = "" });
    }
}
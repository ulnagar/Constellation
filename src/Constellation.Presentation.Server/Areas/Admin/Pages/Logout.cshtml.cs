namespace Constellation.Presentation.Server.Areas.Admin.Pages;

using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
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

    public async Task<IActionResult> OnGet(string returnUrl = null)
    {
        await _signInManager.SignOutAsync();

        HttpContext.Session.Clear();

        if (returnUrl is not null)
        {
            return LocalRedirect(returnUrl);
        }
         
        return RedirectToPage("/Index", new { area = ""});
    }

    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
        await _signInManager.SignOutAsync();

        HttpContext.Session.Clear();
        
        if (returnUrl is not null)
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToPage("/");
    }
}
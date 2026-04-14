namespace Constellation.Presentation.Server.Pages.Auth;

using BaseModels;
using Constellation.Application.Models.Identity;
using Core.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[AllowAnonymous]
public class CompleteSSOModel : BasePageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ILogger _logger;

    public CompleteSSOModel(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ILogger logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger
            .ForContext<CompleteSSOModel>();
    }

    public async Task<IActionResult> OnGet()
    {
        // Redirect to home page
        return RedirectToPage("/Index");
    }

    public async Task<IActionResult> OnPost()
    {

        return RedirectToPage("/");
    }
}
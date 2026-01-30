namespace Constellation.Presentation.Server.Pages.Auth;

using BaseModels;
using Constellation.Application.Models.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

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
        var accessToken = await HttpContext.GetTokenAsync(
            OpenIdConnectDefaults.AuthenticationScheme, "access_token");

        if (string.IsNullOrWhiteSpace(accessToken))
            return RedirectToPage("/Auth/Login");

        if (accessToken.StartsWith("Bearer "))
        {
            accessToken = accessToken.Substring("Bearer ".Length).Trim();
        }

        var handler = new JwtSecurityTokenHandler();

        // Check if the token format is valid (optional)
        if (!handler.CanReadToken(accessToken))
        {
            // Handle invalid token format
            return RedirectToPage("/Auth/Login");
        }

        var jsonToken = handler.ReadJwtToken(accessToken);

        // Access token properties
        var claims = jsonToken.Claims;
        var email = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

        if (string.IsNullOrWhiteSpace(email))
            return RedirectToPage("/Auth/Login");

        // Get user entry from database
        AppUser? user = await _userManager.FindByEmailAsync(email);

        if (user is null)
            return RedirectToPage("/Auth/AccessDenied");

        _logger.Information("Found user {user}", user.Email);

        await _signInManager.SignInAsync(user, false);
        
        _logger.Information(" - Login succeeded for {user}", user.Email);

        user.AddLogin(DateTime.UtcNow, Constellation.Application.Models.Identity.Enums.LoginStatus.Success);

        await _userManager.UpdateAsync(user);

        // Redirect to home page
        return RedirectToPage("/Index");
    }
}
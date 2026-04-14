namespace Constellation.Presentation.Server.Helpers.Identity;

using Application.Models.Identity;
using Application.Models.Identity.Enums;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Pages.Auth;
using Serilog;
using Shared.Helpers.Logging;
using System.Security.Claims;

internal static class IdentityHelpers
{
    private static readonly Serilog.ILogger _logger = Log.Logger
        .ForContext<CompleteSSOModel>()
        .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);

    internal static async Task SyncUserWithIdentity(TokenValidatedContext context)
    {
        _logger
            .Debug("Hit SyncUserWithIdentity");

        UserManager<AppUser> userManager = context.HttpContext
            .RequestServices
            .GetRequiredService<UserManager<AppUser>>();

        SignInManager<AppUser> signInManager = context.HttpContext
            .RequestServices
            .GetRequiredService<SignInManager<AppUser>>();

        _logger
            .Debug("Resolved required services");

        foreach (var claim in context.Principal?.Claims ?? [])
        {
            _logger.Information("Claim: {Type} = {Value}", claim.Type, claim.Value);
        }

        // Get the external user's identifier (typically 'sub' claim)
        string? externalUserId = context.Principal?
            .FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.Principal?.FindFirstValue("sub");

        string? email = context.Principal?
            .FindFirstValue(ClaimTypes.Name);

        _logger
            .ForContext(ClaimTypes.NameIdentifier, externalUserId)
            .ForContext(ClaimTypes.Email, email)
            .Debug("Tried to retrieve user claims");

        if (externalUserId is null || email is null)
        {
            // Not enough information was found to process login
            context.HandleResponse();
            context.Response.Redirect("/Auth/AccessDeniedSSO");

            _logger
                .ForContext(ClaimTypes.NameIdentifier, externalUserId)
                .ForContext(ClaimTypes.Email, email)
                .Warning("SSO Login: Did not pass claim nullability checks");

            return;
        }

        _logger
            .Debug("Passed claim nullability checks");

        // Find or create the user in Identity
        AppUser? user = await userManager.FindByLoginAsync("oidc", externalUserId);

        if (user == null)
        {
            user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // User does not exist, and should not be logged in.
                context.HandleResponse();
                context.Response.Redirect("/Auth/AccessDeniedSSO");

                _logger
                    .ForContext(ClaimTypes.NameIdentifier, externalUserId)
                    .ForContext(ClaimTypes.Email, email)
                    .Warning("SSO Login: User cannot be found");

                return;
            }

            // Link external login to Identity user
            await userManager.AddLoginAsync(user, 
                new UserLoginInfo(
                    "oidc", 
                    externalUserId, 
                    "DoE Login"));
        }

        _logger
            .Debug("User found by oidc value or email");
        
        // Sign in with Identity
        user.AddLogin(DateTime.UtcNow, LoginStatus.SingleSignOn);
        await userManager.UpdateAsync(user);
        await signInManager.SignInAsync(user, isPersistent: false);

        _logger
            .ForContext("RedirectUri", context.Properties?.RedirectUri)
            .Debug("User logged in and being redirected");

        context.HandleResponse();
        context.Response.Redirect("/");
    }
}

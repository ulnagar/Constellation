namespace Constellation.Presentation.Server.Helpers.Identity;

using Application.Models.Identity;
using Application.Models.Identity.Enums;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

internal static class IdentityHelpers
{
    internal static async Task SyncUserWithIdentity(TokenValidatedContext context)
    {
        UserManager<AppUser> userManager = context.HttpContext
            .RequestServices
            .GetRequiredService<UserManager<AppUser>>();

        SignInManager<AppUser> signInManager = context.HttpContext
            .RequestServices
            .GetRequiredService<SignInManager<AppUser>>();

        // Get the external user's identifier (typically 'sub' claim)
        string? externalUserId = context.Principal?
            .FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.Principal?.FindFirstValue("sub");

        string? email = context.Principal?
            .FindFirstValue(ClaimTypes.Email);

        if (externalUserId is null || email is null)
            return;

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

                return;
            }

            // Link external login to Identity user
            await userManager.AddLoginAsync(user, 
                new UserLoginInfo(
                    "oidc", 
                    externalUserId, 
                    "DoE Login"));
        }

        // Sign in with Identity
        user.AddLogin(DateTime.UtcNow, LoginStatus.SingleSignOn);
        await userManager.UpdateAsync(user);
        await signInManager.SignInAsync(user, isPersistent: false);

        context.HandleResponse();
        context.Response.Redirect(context.Properties?.RedirectUri ?? "/");
    }
}

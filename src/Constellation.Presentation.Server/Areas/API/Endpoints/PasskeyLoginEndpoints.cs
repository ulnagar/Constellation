namespace Constellation.Presentation.Server.Areas.API.Endpoints;

using Constellation.Infrastructure.Persistence.ConstellationContext;
using Core.Models.Auth;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Buffers.Text;
using System.Text;

public static class PasskeyLoginEndpoints
{
    public static RouteGroupBuilder MapPasskeyLogin(this RouteGroupBuilder group)
    {
        RouteGroupBuilder login = group.MapGroup("/login");

        login.MapPost("/options", LoginOptions);
        login.MapPost("/complete", LoginComplete);
        return group;
    }

    private static async Task<IResult> LoginOptions(
        HttpContext context,
        IFido2 fido2)
    {
        await context.Session.LoadAsync();

        GetAssertionOptionsParams credParams = new()
        {
            AllowedCredentials = [],
            UserVerification = UserVerificationRequirement.Required
        };

        AssertionOptions options = fido2.GetAssertionOptions(credParams);

        context.Session.SetString("fido2.assertion.options", options.ToJson());

        return Results.Ok(options);
    }

    private static async Task<IResult> LoginComplete(
        AuthenticatorAssertionRawResponse response,
        HttpContext context,
        IFido2 fido2,
        SignInManager<AppUser> signInManager,
        ConstellationDbContext db)
    {
        await context.Session.LoadAsync();

        string? json = context.Session.GetString("fido2.assertion.options");
        if (json is null) return Results.BadRequest("Login session expired.");

        AssertionOptions options = AssertionOptions.FromJson(json);

        var credentialId = WebEncoders.Base64UrlDecode(response.Id);

        AppUserPasskey? credential = await db.Set<AppUserPasskey>()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.CredentialId == credentialId);

        if (credential is null) return Results.Unauthorized();

        MakeAssertionParams credParams = new()
        {
            AssertionResponse = response,
            OriginalOptions = options,
            StoredPublicKey = credential.PublicKey,
            StoredSignatureCounter = credential.SignatureCounter,
            IsUserHandleOwnerOfCredentialIdCallback = async (args, ct) =>
            {
                byte[] expectedUserId = credential.AppUserId.ToByteArray();
                return args.UserHandle.SequenceEqual(expectedUserId);
            },
            RequestTokenBindingId = []
        };

        VerifyAssertionResult result = await fido2.MakeAssertionAsync(credParams);

        credential.SignatureCounter = result.SignCount;
        await db.SaveChangesAsync();

        await signInManager.SignInAsync(credential.User, isPersistent: true,
            authenticationMethod: "passkey");

        return Results.Ok();
    }
}

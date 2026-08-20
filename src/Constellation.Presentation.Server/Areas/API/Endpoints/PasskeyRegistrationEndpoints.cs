namespace Constellation.Presentation.Server.Areas.API.Endpoints;

using Constellation.Infrastructure.Persistence.ConstellationContext;
using Core.Models.Auth;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;

public static class PasskeyRegistrationEndpoints
{
    public static RouteGroupBuilder MapPasskeyRegistration(this RouteGroupBuilder group)
    {
        RouteGroupBuilder register = group.MapGroup("/register");

        register.MapPost("/options", RegistrationOptions).RequireAuthorization();
        register.MapPost("/complete", RegistrationComplete).RequireAuthorization();
        return group;
    }

    private static async Task<IResult> RegistrationOptions(
        HttpContext context,
        IFido2 fido2,
        UserManager<AppUser> userManager)
    {
        AppUser? user = await userManager.GetUserAsync(context.User);
        if (user is null) return Results.Unauthorized();

        await context.Session.LoadAsync();

        Fido2User fido2User = new Fido2User
        {
            Id = user.Id.ToByteArray(),
            Name = user.UserName,
            DisplayName = user.Email
        };

        List<PublicKeyCredentialDescriptor> existingKeys = user.PasskeyCredentials
            .Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))
            .ToList();

        RequestNewCredentialParams credParams = new()
        {
            User = fido2User,
            ExcludeCredentials = existingKeys,
            AuthenticatorSelection = new()
            {
                ResidentKey = ResidentKeyRequirement.Required,
                UserVerification = UserVerificationRequirement.Required
            }
        };

        CredentialCreateOptions options = fido2.RequestNewCredential(credParams);

        context.Session.SetString("fido2.attestation.options", options.ToJson());

        return Results.Ok(options);
    }

    private static async Task<IResult> RegistrationComplete(
        AuthenticatorAttestationRawResponse response,
        HttpContext context,
        IFido2 fido2,
        UserManager<AppUser> userManager,
        ConstellationDbContext db)
    {
        AppUser? user = await userManager.GetUserAsync(context.User);
        if (user is null) return Results.Unauthorized();

        await context.Session.LoadAsync();

        string? json = context.Session.GetString("fido2.attestation.options");
        if (json is null) return Results.BadRequest("Registration session expired.");

        CredentialCreateOptions options = CredentialCreateOptions.FromJson(json);

        MakeNewCredentialParams credParams = new()
        {
            AttestationResponse = response,
            OriginalOptions = options,
            IsCredentialIdUniqueToUserCallback = async (args, ct) => !await db.Set<AppUserPasskey>()
                .AnyAsync(c => c.CredentialId == args.CredentialId, ct)
        };

        RegisteredPublicKeyCredential result = await fido2.MakeNewCredentialAsync(credParams);

        db.Set<AppUserPasskey>().Add(new AppUserPasskey
        {
            CredentialId = result.Id,
            PublicKey = result.PublicKey,
            SignatureCounter = result.SignCount,
            CredType = result.Type.ToString(),
            AaGuid = result.AaGuid,
            CreatedAt = DateTime.UtcNow,
            AppUserId = user.Id
        });

        await db.SaveChangesAsync();
        return Results.Ok();
    }
}
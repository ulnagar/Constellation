namespace Constellation.Presentation.Server.Areas.API.Endpoints;

using Application.Interfaces.Repositories;
using Application.Models.Identity.Repositories;
using Core.Models.Auth;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

public static class PasskeyRegistrationEndpoints
{
    public static RouteGroupBuilder MapPasskeyRegistration(this RouteGroupBuilder group)
    {
        RouteGroupBuilder register = group.MapGroup("/register");

        register.MapPost("/options", RegistrationOptions).RequireAuthorization();
        register.MapPost("/complete", RegistrationComplete).RequireAuthorization();
        register.MapDelete("/{credentialId}", DeletePasskey).RequireAuthorization();
        return group;
    }

    private static async Task<IResult> RegistrationOptions(
        HttpContext context,
        IFido2 fido2,
        UserManager<AppUser> userManager,
        string name = "My Passkey")
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
        context.Session.SetString("fido2.attestation.name", name);

        return Results.Ok(options);
    }

    private static async Task<IResult> RegistrationComplete(
        AuthenticatorAttestationRawResponse response,
        HttpContext context,
        IFido2 fido2,
        UserManager<AppUser> userManager,
        IIdentityRepository identityRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        AppUser? user = await userManager.GetUserAsync(context.User);
        if (user is null) return Results.Unauthorized();

        await context.Session.LoadAsync(cancellationToken);

        string? json = context.Session.GetString("fido2.attestation.options");
        if (json is null) return Results.BadRequest("Registration session expired.");

        var name = context.Session.GetString("fido2.attestation.name") ?? "My Passkey";

        CredentialCreateOptions options = CredentialCreateOptions.FromJson(json);

        MakeNewCredentialParams credParams = new()
        {
            AttestationResponse = response,
            OriginalOptions = options,
            IsCredentialIdUniqueToUserCallback = async (args, ct) => 
                !await identityRepository.DoesCredentialAlreadyExist(args.CredentialId, ct)
        };

        RegisteredPublicKeyCredential result = await fido2.MakeNewCredentialAsync(credParams, cancellationToken);

        AppUserPasskey passkey = new()
        {
            CredentialId = result.Id,
            PublicKey = result.PublicKey,
            SignatureCounter = result.SignCount,
            CredType = result.Type.ToString(),
            AaGuid = result.AaGuid,
            CreatedAt = DateTime.UtcNow,
            AppUserId = user.Id,
            Name = name
        };

        identityRepository.Insert(passkey);

        await unitOfWork.CompleteAsync(cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> DeletePasskey(
        string credentialId,
        HttpContext context,
        UserManager<AppUser> userManager,
        IIdentityRepository identityRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        AppUser? user = await userManager.GetUserAsync(context.User);
        if (user is null) return Results.Unauthorized();

        // Ensure the user owns this credential before deleting
        AppUserPasskey? credential = await identityRepository.GetPasskeyById(WebEncoders.Base64UrlDecode(credentialId), cancellationToken);

        if (credential is null || credential.AppUserId != user.Id) return Results.NotFound();

        user.PasskeyCredentials.Remove(credential);
        await unitOfWork.CompleteAsync(cancellationToken);

        return Results.Ok();
    }
}
namespace Constellation.Presentation.Server.Areas.API.Endpoints;

using Application.Domains.EnrolmentContext.Interfaces;
using Application.Interfaces.Repositories;
using Application.Models.Identity.Repositories;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Core.Models.EnrolmentContext.EnrolmentUser;
using Core.Models.EnrolmentContext.EnrolmentUser.Repositories;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

public static class PasskeyLoginEndpoints
{
    public static RouteGroupBuilder MapPasskeyLogin(this RouteGroupBuilder group)
    {
        RouteGroupBuilder login = group.MapGroup("/login");

        login.MapPost("/options", LoginOptions);
        login.MapPost("/complete", LoginComplete);
        login.MapPost("/enrolments", LoginEnrolments);
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
        IIdentityRepository identityRepository,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        await context.Session.LoadAsync(cancellationToken);

        string? json = context.Session.GetString("fido2.assertion.options");
        if (json is null) return Results.BadRequest("Login session expired.");

        AssertionOptions options = AssertionOptions.FromJson(json);

        byte[] credentialId = WebEncoders.Base64UrlDecode(response.Id);

        AppUserPasskey? credential = await identityRepository.GetPasskeyById(credentialId, cancellationToken);

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

        VerifyAssertionResult result = await fido2.MakeAssertionAsync(credParams, cancellationToken);

        credential.SignatureCounter = result.SignCount;
        credential.User.AddLogin(DateTime.UtcNow, LoginStatus.Passkey);
        await unitOfWork.CompleteAsync(cancellationToken);

        await signInManager.SignInAsync(credential.User, isPersistent: true,
            authenticationMethod: "passkey");

        return Results.Ok();
    }

    private static async Task<IResult> LoginEnrolments(
        AuthenticatorAssertionRawResponse response,
        HttpContext context,
        IFido2 fido2,
        SignInManager<EnrolmentUser> signInManager,
        IEnrolmentUserRepository enrolmentUserRepository,
        IEnrolmentUnitOfWork unitOfWork,
        CancellationToken cancellationToken = default)
    {
        await context.Session.LoadAsync(cancellationToken);

        string? json = context.Session.GetString("fido2.assertion.options");
        if (json is null) return Results.BadRequest("Login session expired.");

        AssertionOptions options = AssertionOptions.FromJson(json);

        byte[] credentialId = WebEncoders.Base64UrlDecode(response.Id);

        EnrolmentUserPasskey? credential = await enrolmentUserRepository.GetPasskeyById(credentialId, cancellationToken);

        if (credential is null || credential.User is null) 
            return Results.Unauthorized();
        
        MakeAssertionParams credParams = new()
        {
            AssertionResponse = response,
            OriginalOptions = options,
            StoredPublicKey = credential.PublicKey,
            StoredSignatureCounter = credential.SignatureCounter,
            IsUserHandleOwnerOfCredentialIdCallback = async (args, ct) =>
            {
                byte[] expectedUserId = credential.UserId.ToByteArray();
                return args.UserHandle.SequenceEqual(expectedUserId);
            },
            RequestTokenBindingId = []
        };

        VerifyAssertionResult result = await fido2.MakeAssertionAsync(credParams, cancellationToken);

        credential.SignatureCounter = result.SignCount;
        credential.User.AddLogin(DateTime.UtcNow, LoginStatus.Passkey);
        await signInManager.SignInAsync(credential.User, true, "passkey");
        await unitOfWork.CompleteAsync(cancellationToken);

        return Results.Ok();
    }
}

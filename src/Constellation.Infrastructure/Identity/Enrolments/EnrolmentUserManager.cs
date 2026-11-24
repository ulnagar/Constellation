namespace Constellation.Infrastructure.Identity.Enrolments;

using Application.Domains.EnrolmentContext.Helpers;
using Core.Models.EnrolmentContext.EnrolmentUser;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class EnrolmentUserManager : UserManager<EnrolmentUser>
{
    public EnrolmentUserManager(
        IUserStore<EnrolmentUser> store, 
        IOptions<IdentityOptions> optionsAccessor, 
        IPasswordHasher<EnrolmentUser> passwordHasher, 
        IEnumerable<IUserValidator<EnrolmentUser>> userValidators, 
        IEnumerable<IPasswordValidator<EnrolmentUser>> passwordValidators, 
        ILookupNormalizer keyNormalizer, 
        IdentityErrorDescriber errors, 
        IServiceProvider services, 
        ILogger<UserManager<EnrolmentUser>> logger) 
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
    }

    public override Task<string> GenerateEmailConfirmationTokenAsync(EnrolmentUser user) =>
        GenerateUserTokenAsync(user, EnrolmentsIdentityConstants.EmailTokenProvider, EnrolmentsIdentityConstants.EmailConfirmationToken);

    public override async Task<IdentityResult> ConfirmEmailAsync(EnrolmentUser user, string token)
    {
        if (!await VerifyUserTokenAsync(user, EnrolmentsIdentityConstants.EmailTokenProvider, EnrolmentsIdentityConstants.EmailConfirmationToken, token))
            return IdentityResult.Failed(ErrorDescriber.InvalidToken());

        user.EmailConfirmed = true;
        user.ConfirmEmailChange();

        return await UpdateAsync(user);
    }

    public override Task<string> GenerateChangeEmailTokenAsync(EnrolmentUser user, string newEmail) =>
        GenerateUserTokenAsync(user, EnrolmentsIdentityConstants.EmailTokenProvider, EnrolmentsIdentityConstants.EmailConfirmationToken);

    public override async Task<IdentityResult> ChangeEmailAsync(EnrolmentUser user, string newEmail, string token)
    {
        if (!await VerifyUserTokenAsync(user, EnrolmentsIdentityConstants.EmailTokenProvider, EnrolmentsIdentityConstants.EmailConfirmationToken, token))
            return IdentityResult.Failed(ErrorDescriber.InvalidToken());

        user.ConfirmEmailChange();
        
        return await UpdateAsync(user);
    }

    public override Task<string> GenerateChangePhoneNumberTokenAsync(EnrolmentUser user, string phoneNumber) =>
        GenerateUserTokenAsync(user, EnrolmentsIdentityConstants.PhoneTokenProvider, EnrolmentsIdentityConstants.PhoneConfirmationToken);

    public override async Task<IdentityResult> ChangePhoneNumberAsync(EnrolmentUser user, string phoneNumber,
        string token)
    {
        if (!await VerifyUserTokenAsync(user, EnrolmentsIdentityConstants.PhoneTokenProvider, EnrolmentsIdentityConstants.PhoneConfirmationToken, token))
            return IdentityResult.Failed(ErrorDescriber.InvalidToken());

        user.ConfirmPhoneChange();
        
        return await UpdateAsync(user);
    }
}
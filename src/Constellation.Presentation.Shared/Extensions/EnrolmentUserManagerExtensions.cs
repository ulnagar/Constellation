namespace Constellation.Presentation.Shared.Extensions;

using Application.Domains.EnrolmentContext.Helpers;
using Core.Models.EnrolmentContext.EnrolmentUser;
using Microsoft.AspNetCore.Identity;

public static class EnrolmentUserManagerExtensions
{
    public static Task<string> GenerateEmailLoginTokenAsync(
        this UserManager<EnrolmentUser> manager, EnrolmentUser user) =>
        manager.GenerateUserTokenAsync(user, EnrolmentsIdentityConstants.EmailTokenProvider, EnrolmentsIdentityConstants.EmailLoginToken);

    public static Task<bool> VerifyEmailLoginTokenAsync(
        this UserManager<EnrolmentUser> manager, EnrolmentUser user, string token) =>
        manager.VerifyUserTokenAsync(user, EnrolmentsIdentityConstants.EmailTokenProvider, EnrolmentsIdentityConstants.EmailLoginToken, token);

    public static Task<string> GeneratePhoneLoginTokenAsync(
        this UserManager<EnrolmentUser> manager, EnrolmentUser user) =>
        manager.GenerateUserTokenAsync(user, EnrolmentsIdentityConstants.PhoneTokenProvider, EnrolmentsIdentityConstants.SmsLoginToken);

    public static Task<bool> VerifyPhoneLoginTokenAsync(
        this UserManager<EnrolmentUser> manager, EnrolmentUser user, string token) =>
        manager.VerifyUserTokenAsync(user, EnrolmentsIdentityConstants.PhoneTokenProvider, EnrolmentsIdentityConstants.SmsLoginToken, token);
}
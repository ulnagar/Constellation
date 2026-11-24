namespace Constellation.Application.Domains.EnrolmentContext.Helpers;

public static class EnrolmentsIdentityConstants
{
    // Auth Scheme
    public const string AuthenticationScheme = "EnrolmentsPortal";

    // Token Providers
    public const string EmailTokenProvider = "EnrolmentsEmail";
    public const string PhoneTokenProvider = "EnrolmentsPhone";

    // Token Purposes
    public const string EmailConfirmationToken = "EnrolmentsEmailConfirmation";
    public const string PhoneConfirmationToken = "EnrolmentsPhoneConfirmation";
    public const string SmsLoginToken = "EnrolmentsSmsLogin";
    public const string EmailLoginToken = "EnrolmentsEmailLogin";
}
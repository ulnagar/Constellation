namespace Constellation.Core.Models.EnrolmentContext.EnrolmentUser.Errors;

using Shared;

public static class EnrolmentUserErrors
{
    public static readonly Error PreferredNameRequired = new(
        "PortalAccount.PreferredNameRequired",
        "A preferred name is required.");

    public static readonly Error EmailRequired = new(
        "PortalAccount.EmailRequired",
        "An email address is required.");

    public static readonly Error EmailUnchanged = new(
        "PortalAccount.EmailUnchanged",
        "The new email address must be different from the current one.");

    public static readonly Error PhoneRequired = new(
        "PortalAccount.PhoneRequired",
        "A phone number is required.");

    public static readonly Error PhoneUnchanged = new(
        "PortalAccount.PhoneUnchanged",
        "The new phone number must be different from the current one.");
}

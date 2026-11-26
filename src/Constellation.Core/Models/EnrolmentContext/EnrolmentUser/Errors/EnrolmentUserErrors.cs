namespace Constellation.Core.Models.EnrolmentContext.EnrolmentUser.Errors;

using Shared;
using ValueObjects;

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

    public static readonly Func<Guid, Error> NotFound = userId => new(
        "PortalAccount.NotFound",
        $"No portal account was found with the ID '{userId}'.");

    public static readonly Func<EmailAddress, Error> NotFoundByEmail = email => new(
        "PortalAccount.NotFoundByEmail",
        $"No portal account was found with the Email '{email}'.");
}

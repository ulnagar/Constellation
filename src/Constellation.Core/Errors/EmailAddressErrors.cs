namespace Constellation.Core.Errors;

using Shared;

public static class EmailAddressErrors
{
    public static readonly Error EmailEmpty = new(
        "EmailAddress.EmailEmpty",
        "Email Address must not be empty.");

    public static readonly Error EmailInvalid = new(
        "EmailAddress.EmailInvalid",
        "Email Address is not valid.");
}
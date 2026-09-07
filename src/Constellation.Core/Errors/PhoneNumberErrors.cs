namespace Constellation.Core.Errors;

using Shared;

public static class PhoneNumberErrors
{
    public static readonly Error NumberEmpty = new(
        "PhoneNumber.NumberEmpty",
        "Phone Number must not be empty");

    public static readonly Error NumberInvalid = new(
        "PhoneNumber.NumberInvalid",
        "Phone Number is not valid");
}
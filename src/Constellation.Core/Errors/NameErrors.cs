namespace Constellation.Core.Errors;

using Shared;

public static class NameErrors
{
    public static readonly Error FirstNameEmpty = new(
        "Name.FirstNameEmpty",
        "First Name must not be empty.");

    public static readonly Error LastNameEmpty = new(
        "Name.LastNameEmpty",
        "Last Name must not be empty.");
}
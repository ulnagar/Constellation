namespace Constellation.Core.Models.Students.Errors;

using Shared;

public static class GenderErrors
{
    public static readonly Error InvalidValue = new(
        "Gender.InvalidValue",
        "The provided Gender is invalid");
}
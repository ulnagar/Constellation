namespace Constellation.Core.Models.Identifiers;

using Errors;
using Primitives;
using Shared;

public readonly record struct SchoolCode(string Value)
    : IStronglyTypedId
{
    public static SchoolCode Empty => new("0000");

    public string Value { get; init; } = Validate(Value);

    public static SchoolCode FromValue(string value) =>
        new(value);

    public override string ToString() =>
        Value;

    public static Result<SchoolCode> TryFromValue(string value)
    {
        string? validation = CheckValue(value);
        if (validation is not null)
            return Result.Failure<SchoolCode>(DomainErrors.Partners.School.InvalidValue);

        return new SchoolCode(value);
    }

    private static string Validate(string value)
    {
        string? validation = CheckValue(value);
        if (validation is not null)
            return Empty.Value;

        return value;
    }

    private static string? CheckValue(string value)
    {
        if (value is not { Length: 4 })
            return "School Code must be exactly four characters";

        if (!value.All(char.IsDigit))
            return "School Code must contain only digits";

        return null;
    }
}
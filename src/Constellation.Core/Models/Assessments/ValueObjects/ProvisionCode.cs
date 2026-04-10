namespace Constellation.Core.Models.Assessments.ValueObjects;

using Constellation.Core.Shared;
using Errors;
using Primitives;

public sealed class ProvisionCode : ValueObject<ProvisionCode, string>, IValueObject<ProvisionCode, string>
{
    public static readonly ProvisionCode Empty = new(string.Empty);
    
    private ProvisionCode() { }

    private ProvisionCode(string value)
    {
        Value = value;
    }

    public static Result<ProvisionCode> Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<ProvisionCode>(ProvisionCodeErrors.EmptyValue);

        return new ProvisionCode();
    }

    public override string ToString() => Value;

    /// <summary>
    /// Do not use. For EF Core only.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static ProvisionCode FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Empty;

        return new(value);
    }
}
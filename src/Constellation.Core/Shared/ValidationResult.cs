namespace Constellation.Core.Shared;

using System.Text.Json.Serialization;

public sealed class ValidationResult : Result, IValidationResult
{
    [JsonConstructor]
    private ValidationResult(Error[] errors)
        : base(false, IValidationResult.ValidationError) =>
        Errors = errors;

    public Error[] Errors { get; }

    public static ValidationResult WithErrors(Error[] errors) => new(errors);
}

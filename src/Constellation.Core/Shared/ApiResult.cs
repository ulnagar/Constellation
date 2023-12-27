#nullable enable
namespace Constellation.Core.Shared;

using System.Collections.Generic;
using System.Linq;

public class ApiResult
{
    public ApiResult() { }

    public bool IsSuccess { get; set; }
    public Error? Error { get; set; }

    public static ApiResult FromResult(Result result)
    {
        if (result.GetType() == typeof(ValidationResult))
        {
            ValidationResult validationResult = (result as ValidationResult)!;

            IEnumerable<string> errors = validationResult.Errors.Select(error => error.Message);

            string errorText = string.Join(", ", errors);

            Shared.Error validationError = new(result.Error.Code, errorText);

            return new() { IsSuccess = validationResult.IsSuccess, Error = validationError };
        }

        return new() { IsSuccess = result.IsSuccess, Error = result.Error };
    }

    public static ApiResult<TValue> FromResult<TValue>(Result<TValue> result)
    {
        if (result.GetType() == typeof(ValidationResult<TValue>))
        {
            ValidationResult<TValue> validationResult = (result as ValidationResult<TValue>)!;

            IEnumerable<string> errors = validationResult.Errors.Select(error => error.Message);

            string errorText = string.Join(", ", errors);

            Shared.Error validationError = new(result.Error.Code, errorText);

            return new() { IsSuccess = validationResult.IsSuccess, Error = validationError, Value = result.Value };
        }

        return new() { IsSuccess = result.IsSuccess, Error = result.Error, Value = result.Value };
    }
}

public class ApiResult<TValue> : ApiResult
{
    public TValue? Value { get; set; }

    public ApiResult() { }
}
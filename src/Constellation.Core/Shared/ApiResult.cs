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
        if (result is ValidationResult validationResult)
        {
            IEnumerable<string> errors = validationResult.Errors.Select(error => error.Message);

            string errorText = string.Join(", ", errors);

            Shared.Error validationError = new(validationResult.Error.Code, errorText);

            return new() { IsSuccess = validationResult.IsSuccess, Error = validationError };
        }

        if (result.IsSuccess)
            return new() { IsSuccess = true };

        return new() { IsSuccess = result.IsSuccess, Error = result.Error };
    }

    public static ApiResult<TValue> FromResult<TValue>(Result<TValue> result)
    {
        if (result is ValidationResult<TValue> validationResult)
        {
            IEnumerable<string> errors = validationResult.Errors.Select(error => error.Message);

            string errorText = string.Join(", ", errors);

            Shared.Error validationError = new(validationResult.Error.Code, errorText);

            return new() { IsSuccess = validationResult.IsSuccess, Error = validationError, Value = validationResult.Value };
        }

        if (result.IsSuccess)
            return new() { IsSuccess = true, Value = result.Value };
        
        return new() { IsSuccess = result.IsSuccess, Error = result.Error };
    }
}

public class ApiResult<TValue> : ApiResult
{
    public TValue? Value { get; set; }

    public ApiResult() { }
}
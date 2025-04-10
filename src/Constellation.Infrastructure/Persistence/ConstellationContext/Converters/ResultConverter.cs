#nullable enable
namespace Constellation.Infrastructure.Persistence.ConstellationContext.Converters;

using Core.Primitives;
using Core.Shared;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

internal sealed class ResultConverter : ValueConverter<Result, string?>
{
    private const string _successCode = "Success";
    private const string _errorSeparator = "::";

    public ResultConverter()
        : base(
            result => ResultToString(result),
            value => StringToResult(value),
            new ConverterMappingHints())
    { }

    private static string? ResultToString(Result result) =>
        result is null
            ? null
            : result.IsSuccess
                ? _successCode
                : $"{result.Error.Code}{_errorSeparator}{result.Error.Message}";

    private static Result StringToResult(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (value == _successCode)
            return Result.Success();
        
        string[] split = value.Split(_errorSeparator);

        string code = split[0];
        string message = split[1];

        Error error = new(code, message);

        return Result.Failure(error);
    }

    public override bool ConvertsNulls => true;
}
namespace Constellation.Application.Common.ValidationRules;

using FluentValidation;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, TElement> MustBeValidPhoneNumber<T, TElement>(this IRuleBuilder<T, TElement> ruleBuilder)
    {
        return ruleBuilder
            .Must(number =>
            {
                var value = number?.ToString();
                if (value is null)
                    return false;

                return value.StartsWith("0011") ||
                       (value.Length == 10 && (
                           value.StartsWith("02") || value.StartsWith("03") ||
                           value.StartsWith("04") || value.StartsWith("07") ||
                           value.StartsWith("08") || value.StartsWith("13")));
            })
            .WithMessage("The phone number is not valid.");
    }
}
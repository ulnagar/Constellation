using FluentValidation;

namespace Constellation.Application.Common.ValidationRules
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, TElement> MustBeValidPhoneNumber<T, TElement>(this IRuleBuilder<T, TElement> ruleBuilder)
        {
            return ruleBuilder
                .Must(number => number.ToString().StartsWith("0011") ||
                        number.ToString().Length == 10 && (
                             number.ToString().StartsWith("02") || number.ToString().StartsWith("03") ||
                             number.ToString().StartsWith("04") || number.ToString().StartsWith("07") ||
                             number.ToString().StartsWith("08") || number.ToString().StartsWith("13")))
                .WithMessage($"The phone number is not valid.");
        }
    }
}

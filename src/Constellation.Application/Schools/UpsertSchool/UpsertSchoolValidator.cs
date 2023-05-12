namespace Constellation.Application.Schools.UpsertSchool;

using Constellation.Application.Common.ValidationRules;
using FluentValidation;

public class UpsertSchoolCommandValidator : AbstractValidator<UpsertSchoolCommand>
{
    public UpsertSchoolCommandValidator()
    {
        RuleFor(command => command.Code).NotEmpty().MaximumLength(4);
        RuleFor(command => command.EmailAddress).EmailAddress().When(command => !string.IsNullOrWhiteSpace(command.EmailAddress));
        RuleFor(command => command.PhoneNumber).MustBeValidPhoneNumber().When(command => !string.IsNullOrWhiteSpace(command.PhoneNumber));
    }
}

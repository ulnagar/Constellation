namespace Constellation.Application.Domains.Schools.Commands.UpsertSchool;

using Common.ValidationRules;
using Core.Models.Identifiers;
using FluentValidation;

public class UpsertSchoolCommandValidator : AbstractValidator<UpsertSchoolCommand>
{
    public UpsertSchoolCommandValidator()
    {
        RuleFor(command => command.Code).NotEqual(SchoolCode.Empty);
        RuleFor(command => command.EmailAddress).EmailAddress().When(command => !string.IsNullOrWhiteSpace(command.EmailAddress));
        RuleFor(command => command.PhoneNumber).MustBeValidPhoneNumber().When(command => !string.IsNullOrWhiteSpace(command.PhoneNumber));
    }
}

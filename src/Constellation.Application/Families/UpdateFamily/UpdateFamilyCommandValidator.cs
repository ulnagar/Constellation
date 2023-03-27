namespace Constellation.Application.Families.UpdateFamily;

using FluentValidation;

internal sealed class UpdateFamilyCommandValidator
    : AbstractValidator<UpdateFamilyCommand>
{
    public UpdateFamilyCommandValidator()
    {
        RuleFor(command => command.FamilyTitle)
            .NotEmpty();

        RuleFor(command => command.AddressLine1)
            .NotEmpty();

        RuleFor(command => command.AddressTown)
            .NotEmpty();

        RuleFor(command => command.AddressPostCode)
            .NotEmpty();

        RuleFor(command => command.FamilyEmail)
            .NotEmpty()
            .EmailAddress();
    }
}

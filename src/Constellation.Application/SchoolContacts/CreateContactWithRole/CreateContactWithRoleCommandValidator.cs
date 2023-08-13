namespace Constellation.Application.SchoolContacts.CreateContactWithRole;

using FluentValidation;

public class CreateContactWithRoleCommandValidator : AbstractValidator<CreateContactWithRoleCommand>
{
    public CreateContactWithRoleCommandValidator()
    {
        RuleFor(command => command.FirstName).NotEmpty();
        RuleFor(command => command.LastName).NotEmpty();
        RuleFor(command => command.EmailAddress).NotEmpty().EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);
        RuleFor(command => command.SchoolCode).NotEmpty().Length(4);
    }
}

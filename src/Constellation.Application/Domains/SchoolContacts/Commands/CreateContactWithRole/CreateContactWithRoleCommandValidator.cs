namespace Constellation.Application.Domains.SchoolContacts.Commands.CreateContactWithRole;

using FluentValidation;

public sealed class CreateContactWithRoleCommandValidator : AbstractValidator<CreateContactWithRoleCommand>
{
    public CreateContactWithRoleCommandValidator()
    {
        RuleFor(command => command.FirstName).NotEmpty();
        RuleFor(command => command.LastName).NotEmpty();
    }
}

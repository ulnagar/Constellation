using FluentValidation;
using MediatR;

namespace Constellation.Application.Features.Partners.SchoolContacts.Commands
{
    public class CreateNewSchoolContactWithRoleCommand : IRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Position { get; set; }
        public string SchoolCode { get; set; }
    }

    public class CreateNewSchoolContactWithRoleCommandValidator : AbstractValidator<CreateNewSchoolContactWithRoleCommand>
    {
        public CreateNewSchoolContactWithRoleCommandValidator()
        {
            RuleFor(command => command.FirstName).NotEmpty();
            RuleFor(command => command.LastName).NotEmpty();
            RuleFor(command => command.EmailAddress).NotEmpty().EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);
            RuleFor(command => command.SchoolCode).NotEmpty().Length(4);
        }
    }
}

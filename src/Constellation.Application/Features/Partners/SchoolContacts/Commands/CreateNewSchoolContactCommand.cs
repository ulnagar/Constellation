using FluentValidation;
using MediatR;

namespace Constellation.Application.Features.Partners.SchoolContacts.Commands
{
    public class CreateNewSchoolContactCommand : IRequest<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public bool SelfRegistered { get; set; }
    }

    public class CreateNewSchoolContactCommandValidator : AbstractValidator<CreateNewSchoolContactCommand>
    {
        public CreateNewSchoolContactCommandValidator()
        {
            RuleFor(command => command.FirstName).NotEmpty();
            RuleFor(command => command.LastName).NotEmpty();
            RuleFor(command => command.EmailAddress).NotEmpty().EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);
        }
    }
}

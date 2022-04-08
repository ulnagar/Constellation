using Constellation.Application.Common.ValidationRules;
using FluentValidation;
using MediatR;

namespace Constellation.Application.Features.Partners.Schools.Commands
{
    public class UpsertSchool : IRequest<ValidateableResponse>, IValidatable
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Town { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Website { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public bool LateOpening { get; set; }
    }

    public class UpsertSchoolValidator : AbstractValidator<UpsertSchool>
    {
        public UpsertSchoolValidator()
        {
            RuleFor(command => command.Code).NotEmpty().MaximumLength(4);
            RuleFor(command => command.EmailAddress).EmailAddress().When(command => !string.IsNullOrWhiteSpace(command.EmailAddress));
            RuleFor(command => command.PhoneNumber).MustBeValidPhoneNumber().When(command => !string.IsNullOrWhiteSpace(command.PhoneNumber));
        }
    }
}

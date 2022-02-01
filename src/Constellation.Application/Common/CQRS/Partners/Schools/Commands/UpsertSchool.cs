using Constellation.Application.Common.ValidationRules;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Partners.Schools.Commands
{
    public class UpsertSchool : IRequest
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

    public class UpsertSchoolHandler : IRequestHandler<UpsertSchool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpsertSchoolHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpsertSchool request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.Schools.ForEditAsync(request.Code);
            var newSchool = false;

            if (entity == null)
            {
                entity = new School();
                newSchool = true;
            }

            entity.Code = request.Code;
            entity.Name = request.Name;
            entity.Address = request.Address;
            entity.Town = request.Town;
            entity.State = request.State;
            entity.PostCode = request.PostCode;
            entity.EmailAddress = request.EmailAddress;
            entity.PhoneNumber = request.PhoneNumber;
            entity.Website = request.Website;
            entity.Longitude = request.Longitude;
            entity.Latitude = request.Latitude;
            entity.HeatSchool = request.LateOpening;

            if (newSchool)
                _unitOfWork.Add(entity);

            await _unitOfWork.CompleteAsync();

            return Unit.Value;
        }
    }
}

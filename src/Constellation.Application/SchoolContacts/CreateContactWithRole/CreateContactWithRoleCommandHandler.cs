namespace Constellation.Application.SchoolContacts.CreateContactWithRole;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.SchoolContacts.Repositories;
using Constellation.Core.Shared;
using Core.Errors;
using Core.Models;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateContactWithRoleCommandHandler 
    : ICommandHandler<CreateContactWithRoleCommand>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateContactWithRoleCommandHandler(
        ISchoolContactRepository contactRepository,
        ISchoolRepository schoolRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _schoolRepository = schoolRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateContactWithRoleCommand>();
    }

    public async Task<Result> Handle(CreateContactWithRoleCommand request, CancellationToken cancellationToken)
    {
        School school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(CreateContactWithRoleCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.SchoolCode), true)
                .Warning("Failed to create new School Contact");

            return Result.Failure(DomainErrors.Partners.School.NotFound(request.SchoolCode));
        }

        SchoolContact existingContact = await _contactRepository.GetWithRolesByEmailAddress(request.EmailAddress, cancellationToken);

        if (existingContact is null)
        {
            Result<SchoolContact> contact = SchoolContact.Create(
                request.FirstName,
                request.LastName,
                request.EmailAddress,
                request.PhoneNumber,
                request.SelfRegistered);

            if (contact.IsFailure)
            {
                _logger
                    .ForContext(nameof(CreateContactWithRoleCommand), request, true)
                    .ForContext(nameof(Error), contact.Error, true)
                    .Warning("Failed to create new School Contact");

                return Result.Failure(contact.Error);
            }

            contact.Value.AddRole(
                request.Position,
                request.SchoolCode,
                school.Name,
                request.Note);

            _contactRepository.Insert(contact.Value);

            await _unitOfWork.CompleteAsync(cancellationToken);
            
            return Result.Success();
        }

        if (existingContact.IsDeleted)
            existingContact.Reinstate();

        if (existingContact.Assignments.Any(role =>
                !role.IsDeleted &&
                role.SchoolCode != request.SchoolCode &&
                role.Role != request.Position))
            return Result.Success();

        existingContact.AddRole(
            request.Position,
            request.SchoolCode,
            school.Name,
            request.Note);

        await _unitOfWork.CompleteAsync(cancellationToken);
        
        return Result.Success();
    }
}

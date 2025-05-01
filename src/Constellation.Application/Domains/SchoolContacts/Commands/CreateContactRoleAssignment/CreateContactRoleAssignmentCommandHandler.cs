namespace Constellation.Application.Domains.SchoolContacts.Commands.CreateContactRoleAssignment;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateContactRoleAssignmentCommandHandler
    : ICommandHandler<CreateContactRoleAssignmentCommand>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateContactRoleAssignmentCommandHandler(
        ISchoolContactRepository contactRepository,
        ISchoolRepository schoolRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _schoolRepository = schoolRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateContactRoleAssignmentCommand>();
    }

    public async Task<Result> Handle(CreateContactRoleAssignmentCommand request, CancellationToken cancellationToken)
    {
        School school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(CreateContactRoleAssignmentCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.SchoolCode), true)
                .Warning("Failed to create School Contact Role");

            return Result.Failure(DomainErrors.Partners.School.NotFound(request.SchoolCode));
        }
        
        SchoolContact contact = await _contactRepository.GetById(request.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger.Warning("Could not find School Contact with Id {id}", request.ContactId);

            return Result.Failure(SchoolContactErrors.NotFound(request.ContactId));
        }

        if (contact.IsDeleted)
            contact.Reinstate();

        if (contact.Assignments.Any(role =>
                !role.IsDeleted &&
                role.Role == request.Position &&
                role.SchoolCode == request.SchoolCode))
            return Result.Success();

        contact.AddRole(
            request.Position,
            request.SchoolCode,
            school.Name,
            request.Note);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}

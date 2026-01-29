namespace Constellation.Application.Domains.Students.Events.StudentWithdrawnDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Enums;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.Models.Operations.Repositories;
using Core.Models.Students.Events;
using Core.ValueObjects;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveSchoolwideTeamsAccess 
    : IDomainEventHandler<StudentWithdrawnDomainEvent>
{
    private readonly ILogger _logger;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeamOperationRepository _operationsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveSchoolwideTeamsAccess(
        IStudentRepository studentRepository,
        ITeamOperationRepository operationsRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _logger = logger.ForContext<StudentWithdrawnDomainEvent>();
        _studentRepository = studentRepository;
        _operationsRepository = operationsRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task Handle(StudentWithdrawnDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to remove student ({studentId}) from school wide teams due to withdrawal", notification.StudentId);

        Student? student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student == null)
        {
            _logger.Warning("Could not find student with Id {studentId} to remove from school wide teams", notification.StudentId);
            return;
        }

        if (student.EmailAddress == EmailAddress.None)
        {
            _logger.Warning("Student does not have valid email address to remove from school wide teams");
            return;
        }

        ModifyTeamMembershipTeamOperation operation = new(
            MicrosoftTeam.StudentsTeamId,
            student.EmailAddress,
            TeamAction.Remove);

        _operationsRepository.Insert(operation);
        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.Information("Scheduled student ({studentId}) removal from school wide teams due to withdrawal", notification.StudentId);
    }
}
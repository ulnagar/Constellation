namespace Constellation.Application.Students.Events.StudentWithdrawnDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Enums;
using Constellation.Application.Features.Partners.Students.Notifications;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Students;
using Core.Models.Students.Events;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveSchoolwideTeamsAccess 
    : IDomainEventHandler<StudentWithdrawnDomainEvent>
{
    private readonly ILogger _logger;
    private readonly IStudentRepository _studentRepository;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveSchoolwideTeamsAccess(
        IStudentRepository studentRepository,
        IMSTeamOperationsRepository operationsRepository,
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

        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student == null)
        {
            _logger.Warning("Could not find student with Id {studentId} to remove from school wide teams", notification.StudentId);
            return;
        }

        StudentEnrolledMSTeamOperation operation = new()
        {
            StudentId = notification.StudentId,
            TeamName = MicrosoftTeam.Students,
            DateScheduled = DateTime.Now,
            Action = MSTeamOperationAction.Remove,
            PermissionLevel = MSTeamOperationPermissionLevel.Member
        };

        _operationsRepository.Insert(operation);
        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.Information("Scheduled student ({studentId}) removal from school wide teams due to withdrawal", notification.StudentId);
    }
}

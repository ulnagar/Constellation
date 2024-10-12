namespace Constellation.Application.Students.Events.StudentEmailAddressChanged;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Enums;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Events;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.ValueObjects;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddOrUpdateTeamsAccess 
    : IDomainEventHandler<StudentEmailAddressChangedDomainEvent>
{
    private readonly ILogger _logger;
    private readonly IStudentRepository _studentRepository;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddOrUpdateTeamsAccess(
        IStudentRepository studentRepository,
        IMSTeamOperationsRepository operationsRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _logger = logger.ForContext<StudentEmailAddressChangedDomainEvent>();
        _studentRepository = studentRepository;
        _operationsRepository = operationsRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task Handle(StudentEmailAddressChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to add student ({studentId}) from school wide teams", notification.StudentId);

        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student == null)
        {
            _logger.Warning("Could not find student with Id {studentId} to add to school wide teams", notification.StudentId);
            return;
        }

        if (student.EmailAddress == EmailAddress.None)
        {
            _logger.Warning("Student with id {StudentId} does not have a valid email address to add to school wide teams", notification.StudentId);
            return;
        }

        StudentEnrolledMSTeamOperation operation = new()
        {
            StudentId = notification.StudentId,
            TeamName = MicrosoftTeam.Students,
            DateScheduled = DateTime.Now,
            Action = MSTeamOperationAction.Add,
            PermissionLevel = MSTeamOperationPermissionLevel.Member
        };

        _operationsRepository.Insert(operation);
        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.Information("Scheduled student ({studentId}) addition to school wide teams", notification.StudentId);
    }
}
namespace Constellation.Application.Domains.Students.Events.StudentEmailAddressChanged;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Enums;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Events;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.ValueObjects;
using Core.Abstractions.Clock;
using Core.Extensions;
using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.Models.Operations.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddOrUpdateTeamsAccess 
    : IDomainEventHandler<StudentEmailAddressChangedDomainEvent>
{
    private readonly ILogger _logger;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeamOperationRepository _operationsRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public AddOrUpdateTeamsAccess(
        IStudentRepository studentRepository,
        ITeamOperationRepository operationsRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _logger = logger.ForContext<StudentEmailAddressChangedDomainEvent>();
        _studentRepository = studentRepository;
        _operationsRepository = operationsRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
    }
    
    public async Task Handle(StudentEmailAddressChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to add student ({studentId}) from school wide teams", notification.StudentId);

        Student? student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

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

        // Remove old email address
        if (!string.IsNullOrEmpty(notification.OldAddress))
        {
            EmailAddress oldEmailAddress = EmailAddress.FromValue(notification.OldAddress);

            ModifyTeamMembershipTeamOperation removeOperation = new(
                MicrosoftTeam.StudentsTeamId,
                oldEmailAddress,
                TeamAction.Remove);

            _operationsRepository.Insert(removeOperation);
        }

        // Add new email address
        ModifyTeamMembershipTeamOperation operation = new(
            MicrosoftTeam.StudentsTeamId,
            student.EmailAddress,
            TeamAction.AddMember);

        _operationsRepository.Insert(operation);

        if (student.CurrentEnrolment is null)
        {
            _logger.Warning("Student with id {StudentId} does not have a valid grade to add to school wide teams", notification.StudentId);
            return;
        }

        string channelName = $"{_dateTime.CurrentYear} - {student.CurrentEnrolment?.Grade.AsName()}";

        ModifyTeamChannelMembershipTeamOperation channelOperation = new(
            MicrosoftTeam.StudentsTeamId,
            channelName,
            student.EmailAddress,
            TeamAction.AddMember);

        _operationsRepository.Insert(channelOperation);

        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.Information("Scheduled student ({studentId}) addition to school wide teams", notification.StudentId);
    }
}
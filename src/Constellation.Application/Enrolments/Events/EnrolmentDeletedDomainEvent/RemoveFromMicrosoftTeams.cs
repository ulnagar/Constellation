namespace Constellation.Application.Enrolments.Events.EnrolmentDeletedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Enrolments.Events;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Models.Students.Errors;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveFromMicrosoftTeams
    : IDomainEventHandler<EnrolmentDeletedDomainEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IMSTeamOperationsRepository _operationRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public RemoveFromMicrosoftTeams(
        IUnitOfWork unitOfWork,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        IMSTeamOperationsRepository operationRepository,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _operationRepository = operationRepository;
        _dateTime = dateTime;
        _logger = logger.ForContext<EnrolmentDeletedDomainEvent>();
    }

    public async Task Handle(EnrolmentDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(EnrolmentDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(notification.StudentId))
                .Error("Failed to complete the event handler");

            return;
        }

        Offering offering = await _offeringRepository.GetById(notification.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(EnrolmentDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(notification.OfferingId))
                .Error("Failed to complete the event handler");

            return;
        }

        StudentMSTeamOperation operation = new()
        {
            StudentId = student.Id,
            OfferingId = offering.Id,
            DateScheduled = _dateTime.Now,
            Action = MSTeamOperationAction.Remove,
            PermissionLevel = MSTeamOperationPermissionLevel.Member
        };

        _operationRepository.Insert(operation);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}

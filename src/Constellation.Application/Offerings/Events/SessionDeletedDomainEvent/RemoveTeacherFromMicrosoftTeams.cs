namespace Constellation.Application.Offerings.Events.SessionDeletedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Models.Subjects.Events;
using Constellation.Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveTeacherFromMicrosoftTeams
    : IDomainEventHandler<SessionDeletedDomainEvent>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveTeacherFromMicrosoftTeams(
        IOfferingRepository offeringRepository,
        IMSTeamOperationsRepository operationsRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _operationsRepository = operationsRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<SessionDeletedDomainEvent>();
    }

    public async Task Handle(SessionDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        Offering offering = await _offeringRepository.GetById(notification.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(SessionDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(notification.OfferingId))
                .Error("Failed to complete the event handler");

            return;
        }

        Session session = offering.Sessions.FirstOrDefault(session => session.Id == notification.SessionId);

        if (session is null)
        {
            _logger
                .ForContext(nameof(SessionDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), SessionErrors.NotFound(notification.SessionId))
                .Error("Failed to complete the event handler");

            return;
        }

        int otherSessions = offering.Sessions.Count(innerSession => !innerSession.IsDeleted && innerSession.StaffId == session.StaffId);
        if (otherSessions > 0)
        {
            _logger
                .ForContext(nameof(SessionDeletedDomainEvent), notification, true)
                .Error("Another non-deleted session exists with this teacher. Do not process event handler.");
        }

        TeacherMSTeamOperation operation = new()
        {
            OfferingId = offering.Id,
            StaffId = session.StaffId,
            Action = MSTeamOperationAction.Remove,
            PermissionLevel = MSTeamOperationPermissionLevel.Owner,
            DateScheduled = _dateTime.Now
        };

        _operationsRepository.Insert(operation);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}

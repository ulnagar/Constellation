namespace Constellation.Application.Offerings.Events.SessionDeletedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveStudentsFromAdobeConnect
    : IDomainEventHandler<SessionDeletedDomainEvent>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IAdobeConnectOperationsRepository _operationsRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveStudentsFromAdobeConnect(
        IOfferingRepository offeringRepository,
        IEnrolmentRepository enrolmentRepository,
        IAdobeConnectOperationsRepository operationsRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _enrolmentRepository = enrolmentRepository;
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

        int otherSessions = offering.Sessions.Count(innerSession => !innerSession.IsDeleted);

        if (otherSessions > 0)
        {
            _logger
                .ForContext(nameof(SessionDeletedDomainEvent), notification, true)
                .Error("Another non-deleted session exists. Do not process event handler.");
        }

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByOfferingId(offering.Id, cancellationToken);

        List<string> studentIds = enrolments.Select(enrolment => enrolment.StudentId).ToList();

        foreach (string studentId in studentIds)
        {
            StudentAdobeConnectOperation operation = new()
            {
                ScoId = session.RoomId,
                StudentId = studentId,
                Action = AdobeConnectOperationAction.Remove,
                DateScheduled = _dateTime.Now
            };

            _operationsRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}

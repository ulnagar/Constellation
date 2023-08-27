namespace Constellation.Application.Offerings.Events.AllSessionsDeletedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Models.Subjects.Events;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveStudentsFromAdobeConnect
    : IDomainEventHandler<AllSessionsDeletedDomainEvent>
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
        _logger = logger.ForContext<AllSessionsDeletedDomainEvent>();
    }

    public async Task Handle(AllSessionsDeletedDomainEvent notification, CancellationToken cancellationToken)
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

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByOfferingId(offering.Id, cancellationToken);

        List<string> studentIds = enrolments.Select(enrolment => enrolment.StudentId).ToList();

        foreach (string roomId in notification.RoomIds)
        {
            foreach (string studentId in studentIds)
            {
                StudentAdobeConnectOperation operation = new()
                {
                    ScoId = roomId,
                    StudentId = studentId,
                    Action = AdobeConnectOperationAction.Remove,
                    DateScheduled = _dateTime.Now
                };

                _operationsRepository.Insert(operation);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}

namespace Constellation.Application.Offerings.Events.AllSessionsDeletedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Models.Subjects.Events;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveTeachersFromCanvas
    : IDomainEventHandler<AllSessionsDeletedDomainEvent>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICanvasOperationsRepository _operationsRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveTeachersFromCanvas(
        IOfferingRepository offeringRepository,
        ICanvasOperationsRepository operationsRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
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

        foreach (string staffId in notification.StaffIds)
        {
            ModifyEnrolmentCanvasOperation operation = new()
            {
                UserId = staffId,
                CourseId = $"{offering.EndDate.Year}-{offering.Name.Substring(0, offering.Name.Length - 1)}",
                Action = CanvasOperation.EnrolmentAction.Remove,
                ScheduledFor = _dateTime.Now
            };

            _operationsRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}

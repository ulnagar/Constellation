namespace Constellation.Application.Offerings.Events.TeacherRemovedFromOfferingDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveTeacherFromCanvasCourse
    : IDomainEventHandler<TeacherRemovedFromOfferingDomainEvent>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICanvasOperationsRepository _operationsRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveTeacherFromCanvasCourse(
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
        _logger = logger.ForContext<TeacherRemovedFromOfferingDomainEvent>();
    }

    public async Task Handle(TeacherRemovedFromOfferingDomainEvent notification, CancellationToken cancellationToken)
    {
        Offering offering = await _offeringRepository.GetById(notification.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(TeacherRemovedFromOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(notification.OfferingId))
                .Error("Failed to complete the event handler");

            return;
        }

        TeacherAssignment assignment = offering.Teachers.FirstOrDefault(assignment => assignment.Id == notification.AssignmentId);

        if (assignment is null)
        {
            _logger
                .ForContext(nameof(TeacherRemovedFromOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), TeacherAssignmentErrors.NotFound(notification.AssignmentId))
                .Error("Failed to complete the event handler");

            return;
        }

        foreach (Resource resource in offering.Resources.Where(resource => resource.Type == ResourceType.CanvasCourse))
        {
            ModifyEnrolmentCanvasOperation operation = new()
            {
                UserId = assignment.StaffId,
                //CourseId = $"{offering.EndDate.Year}-{offering.Name.Substring(0, offering.Name.Length - 2)}",
                CourseId = resource.ResourceId,
                Action = CanvasOperation.EnrolmentAction.Remove,
                ScheduledFor = _dateTime.Now
            };

            _operationsRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}

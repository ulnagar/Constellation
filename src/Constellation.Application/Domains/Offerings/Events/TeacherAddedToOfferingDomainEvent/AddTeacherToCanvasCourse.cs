namespace Constellation.Application.Domains.Offerings.Events.TeacherAddedToOfferingDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Operations;
using Constellation.Core.Shared;
using Core.Models.Operations.Enums;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddTeacherToCanvasCourse
    : IDomainEventHandler<TeacherAddedToOfferingDomainEvent>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICanvasOperationsRepository _operationsRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddTeacherToCanvasCourse(
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
        _logger = logger.ForContext<TeacherAddedToOfferingDomainEvent>();
    }

    public async Task Handle(TeacherAddedToOfferingDomainEvent notification, CancellationToken cancellationToken)
    {
        Offering offering = await _offeringRepository.GetById(notification.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(TeacherAddedToOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(notification.OfferingId))
                .Error("Failed to complete the event handler");

            return;
        }

        TeacherAssignment assignment = offering.Teachers.FirstOrDefault(assignment => assignment.Id == notification.AssignmentId);

        if (assignment is null)
        {
            _logger
                .ForContext(nameof(TeacherAddedToOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), TeacherAssignmentErrors.NotFound(notification.AssignmentId))
                .Error("Failed to complete the event handler");

            return;
        }

        List<CanvasCourseResource> resources = offering.Resources
            .OfType<CanvasCourseResource>()
            .ToList();

        foreach (CanvasCourseResource resource in resources)
        {
            ModifyEnrolmentCanvasOperation operation = new(
                assignment.StaffId.ToString(),
                resource.CourseId,
                resource.SectionId,
                CanvasAction.Add,
                CanvasUserType.Teacher,
                _dateTime.Now);

            _operationsRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}

namespace Constellation.Application.Enrolments.Events.EnrolmentDeletedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Enrolments.Events;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Operations;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Models.Canvas.Models;
using Core.Models.Operations.Enums;
using Core.Models.Students.Errors;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal class RemoveFromCanvas
    : IDomainEventHandler<EnrolmentDeletedDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICanvasOperationsRepository _operationRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveFromCanvas(
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ICanvasOperationsRepository operationRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _operationRepository = operationRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
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

        List<CanvasCourseResource> resources = offering.Resources
            .Where(resource => resource.Type == ResourceType.CanvasCourse)
            .Select(resource => resource as CanvasCourseResource)
            .ToList();

        List<Offering> offerings = await _offeringRepository.GetByStudentId(notification.StudentId, cancellationToken);
        List<CanvasCourseCode?> activeCourseIds = offerings
            .SelectMany(offering => offering.Resources)
            .Where(resource => resource.Type == ResourceType.CanvasCourse)
            .Select(resource => resource as CanvasCourseResource)
            .Select(resource => resource?.CourseId)
            .Distinct()
            .ToList();
        
        foreach (CanvasCourseResource resource in resources)
        {
            // Ensure student isn't also enrolled in another current class that is covered by these resources
            // (e.g. student is marked withdrawn from 07SCIP1 but enrolled in 07SCIP2)
            if (activeCourseIds.Contains(resource.CourseId))
                continue;

            ModifyEnrolmentCanvasOperation operation = new(
                student.StudentReferenceNumber.Number,
                resource.CourseId.ToString(),
                CanvasAction.Remove,
                CanvasUserType.Student,
                _dateTime.Now);

            _operationRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}

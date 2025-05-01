namespace Constellation.Application.Domains.Enrolments.Events.EnrolmentCreatedDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Enrolments.Events;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Operations;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Operations.Enums;
using Core.Models.Students.Errors;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddToCanvas
    : IDomainEventHandler<EnrolmentCreatedDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICanvasOperationsRepository _operationRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddToCanvas(
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
        _logger = logger.ForContext<EnrolmentCreatedDomainEvent>();
    }

    public async Task Handle(EnrolmentCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(EnrolmentCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(notification.StudentId), true)
                .Warning("Failed to add student to Canvas course");

            return;
        }

        if (student.StudentReferenceNumber == StudentReferenceNumber.Empty ||
            student.EmailAddress == EmailAddress.None)
        {
            _logger
                .ForContext(nameof(EnrolmentCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentReferenceNumberErrors.EmptyValue, true)
                .Warning("Failed to add student to Canvas course");

            return;
        }

        Offering offering = await _offeringRepository.GetById(notification.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(EnrolmentCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(notification.OfferingId), true)
                .Warning("Failed to add student to Canvas course");

            return;
        }

        if (!offering.IsCurrent && offering.EndDate < _dateTime.Today)
            return;

        List<CanvasCourseResource> resources = offering.Resources
            .Where(resource => resource.Type == ResourceType.CanvasCourse)
            .Select(resource => resource as CanvasCourseResource)
            .ToList();

        foreach (CanvasCourseResource resource in resources)
        {
            ModifyEnrolmentCanvasOperation operation = new(
                student.StudentReferenceNumber.Number,
                resource.CourseId,
                resource.SectionId,
                CanvasAction.Add,
                CanvasUserType.Student,
                offering.IsCurrent ? _dateTime.Now : offering.StartDate.ToDateTime(TimeOnly.MinValue));

            _operationRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}

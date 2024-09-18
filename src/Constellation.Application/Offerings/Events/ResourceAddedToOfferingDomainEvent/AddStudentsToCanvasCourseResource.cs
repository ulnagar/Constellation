namespace Constellation.Application.Offerings.Events.ResourceAddedToOfferingDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Operations;
using Constellation.Core.Shared;
using Core.Models.Enrolments.Repositories;
using Core.Models.Operations.Enums;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddStudentsToCanvasCourseResource
    : IDomainEventHandler<ResourceAddedToOfferingDomainEvent>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly ICanvasOperationsRepository _operationsRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddStudentsToCanvasCourseResource(
        IOfferingRepository offeringRepository,
        IStudentRepository studentRepository,
        ICanvasOperationsRepository operationsRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _studentRepository = studentRepository;
        _operationsRepository = operationsRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ResourceAddedToOfferingDomainEvent>();
    }

    public async Task Handle(ResourceAddedToOfferingDomainEvent notification, CancellationToken cancellationToken)
    {
        if (notification.ResourceType != ResourceType.CanvasCourse)
            return;

        Offering offering = await _offeringRepository.GetById(notification.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(ResourceAddedToOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(notification.OfferingId))
                .Error("Failed to complete the event handler");

            return;
        }

        CanvasCourseResource resource = offering.Resources.FirstOrDefault(resource => resource.Id == notification.ResourceId) as CanvasCourseResource;

        if (resource is null)
        {
            _logger
                .ForContext(nameof(ResourceAddedToOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), ResourceErrors.NotFound(notification.ResourceId))
                .Error("Failed to complete the event handler");

            return;
        }

        List<Student> students = await _studentRepository.GetCurrentEnrolmentsForOffering(offering.Id, cancellationToken);

        foreach (Student student in students)
        {
            ModifyEnrolmentCanvasOperation operation = new(
                student.StudentReferenceNumber.Number,
                resource.CourseId.ToString(),
                CanvasAction.Add,
                CanvasUserType.Student,
                _dateTime.Now);

            _operationsRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}

namespace Constellation.Application.Domains.Offerings.Events.TeacherRemovedFromOfferingDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Operations;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Errors;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Shared;
using Core.Models.Operations.Enums;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveTeacherFromCanvasCourse
    : IDomainEventHandler<TeacherRemovedFromOfferingDomainEvent>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICanvasOperationsRepository _operationsRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveTeacherFromCanvasCourse(
        IOfferingRepository offeringRepository,
        ICanvasOperationsRepository operationsRepository,
        IStaffRepository staffRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _operationsRepository = operationsRepository;
        _staffRepository = staffRepository;
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

        StaffMember staffMember = await _staffRepository.GetById(assignment.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(TeacherAddedToOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(assignment.StaffId))
                .Error("Failed to complete the event handler");

            return;
        }

        List<CanvasCourseResource> resources = offering.Resources
            .OfType<CanvasCourseResource>()
            .ToList();

        foreach (CanvasCourseResource resource in resources)
        {
            ModifyEnrolmentCanvasOperation operation = new(
                staffMember.EmployeeId.Number,
                resource.CourseId,
                resource.SectionId,
                CanvasAction.Remove,
                CanvasUserType.Teacher,
                _dateTime.Now);

            _operationsRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}

namespace Constellation.Application.Assignments.Events.AssignmentAttemptSubmittedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Assignments.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.ValueObjects;
using Core.Abstractions.Clock;
using Core.Errors;
using Core.Models.Assignments;
using Core.Models.Assignments.Services;
using Core.Models.Offerings.Errors;
using Core.Models.Subjects.Errors;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UploadSubmissionToCanvas
    : IDomainEventHandler<AssignmentAttemptSubmittedDomainEvent>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IOfferingRepository _courseOfferingRepository;
    private readonly IAssignmentService _assignmentService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UploadSubmissionToCanvas(
        IAssignmentRepository assignmentRepository,
        IOfferingRepository courseOfferingRepository,
        IAssignmentService assignmentService,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assignmentRepository = assignmentRepository;
        _courseOfferingRepository = courseOfferingRepository;
        _assignmentService = assignmentService;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AssignmentAttemptSubmittedDomainEvent>();
    }

    public async Task Handle(AssignmentAttemptSubmittedDomainEvent notification, CancellationToken cancellationToken)
    {
        CanvasAssignment assignment = await _assignmentRepository.GetById(notification.AssignmentId, cancellationToken);

        if (assignment is null)
        {
            _logger
                .ForContext(nameof(AssignmentAttemptSubmittedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.Assignments.Assignment.NotFound(notification.AssignmentId), true)
                .Warning("Failed to upload Assignment Submission to Canvas");
            return;
        }

        if (assignment.DelayForwarding && assignment.ForwardingDate > _dateTime.Today)
        {
            _logger
                .ForContext(nameof(AssignmentAttemptSubmittedDomainEvent), notification, true)
                .ForContext(nameof(Error), new Error("UploadSubmissionToCanvas", "Assignment set to delay forwarding and delay date has not yet passed"), true)
                .Warning("Failed to upload Assignment Submission to Canvas");

            return;
        }

        CanvasAssignmentSubmission submission = assignment.Submissions.FirstOrDefault(entry => entry.Id == notification.SubmissionId);

        if (submission is null)
        {
            _logger
                .ForContext(nameof(AssignmentAttemptSubmittedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.Assignments.Submission.NotFound(notification.SubmissionId), true)
                .Warning("Failed to upload Assignment Submission to Canvas");
            
            return;
        }

        List<Offering> offerings = await _courseOfferingRepository.GetByCourseId(assignment.CourseId, cancellationToken);

        if (offerings is null)
        {
            _logger
                .ForContext(nameof(AssignmentAttemptSubmittedDomainEvent), notification, true)
                .ForContext(nameof(Error), CourseErrors.NotFound(assignment.CourseId), true)
                .Error("Failed to upload Assignment Submission to Canvas");
            
            return;
        }

        List<string> resources = offerings
            .SelectMany(offering => offering.Resources)
            .Where(resource => resource.Type == ResourceType.CanvasCourse)
            .Select(resource => ((CanvasCourseResource)resource).CourseId)
            .Distinct()
            .ToList();

        if (!resources.Any())
        {
            _logger
                .ForContext(nameof(AssignmentAttemptSubmittedDomainEvent), notification, true)
                .ForContext(nameof(Error), ResourceErrors.NoneOfTypeFound(ResourceType.CanvasCourse), true)
                .Warning("Failed to upload Assignment Submission to Canvas");

            return;
        }

        string canvasCourseId = resources.First();

        if (await _assignmentService.UploadSubmissionToCanvas(assignment, submission, canvasCourseId, cancellationToken))
        {
            assignment.MarkSubmissionUploaded(submission.Id);

            await _unitOfWork.CompleteAsync(cancellationToken);
        }
    }
}

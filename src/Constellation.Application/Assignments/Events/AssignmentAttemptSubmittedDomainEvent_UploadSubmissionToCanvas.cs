namespace Constellation.Application.Assignments.Events;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AssignmentAttemptSubmittedDomainEvent_UploadSubmissionToCanvas
    : IDomainEventHandler<AssignmentAttemptSubmittedDomainEvent>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IOfferingRepository _courseOfferingRepository;
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly ICanvasGateway _canvasGateway;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public AssignmentAttemptSubmittedDomainEvent_UploadSubmissionToCanvas(
        IAssignmentRepository assignmentRepository,
        IOfferingRepository courseOfferingRepository,
        IStoredFileRepository storedFileRepository,
        ICanvasGateway canvasGateway,
        IEmailService emailService,
        Serilog.ILogger logger)
    {
        _assignmentRepository = assignmentRepository;
        _courseOfferingRepository = courseOfferingRepository;
        _storedFileRepository = storedFileRepository;
        _canvasGateway = canvasGateway;
        _emailService = emailService;
        _logger = logger.ForContext<AssignmentAttemptSubmittedDomainEvent>();
    }

    public async Task Handle(AssignmentAttemptSubmittedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Starting UploadSubmissionToCanvas action for Submission ID {id}", notification.SubmissionId.Value);

        var assignment = await _assignmentRepository.GetById(notification.AssignmentId, cancellationToken);

        if (assignment is null)
        {
            _logger.Warning("Could not find assignment with Id {id}", notification.AssignmentId.Value);
            return;
        }

        var submission = assignment.Submissions.FirstOrDefault(entry => entry.Id == notification.SubmissionId);

        if (submission is null)
        {
            _logger.Warning("Could not find a submission with Id {subId} in the assignment {assId}", notification.SubmissionId.Value, notification.AssignmentId.Value);
            return;
        }

        var offerings = await _courseOfferingRepository.GetByCourseId(assignment.CourseId, cancellationToken);

        if (offerings is null)
        {
            _logger.Error("Could not find matching offering for submission {@submission} using Course Id {id}", submission, assignment.CourseId);
            return;
        }

        var offering = offerings.FirstOrDefault(offering => offering.IsCurrent);

        if (offering is null)
        {
            _logger.Error("Could not find matching offering for submission {@submission} from list {@list}", submission, offerings);
            return;
        }

        var canvasCourseId = $"{offering.EndDate.Year}-{offering.Name[..^1]}";

        _logger.Information("Canvas Course Id identified as {id}", canvasCourseId);

        var file = await _storedFileRepository.GetAssignmentSubmissionByLinkId(submission.Id.Value.ToString(), cancellationToken);

        if (file is null)
        {
            _logger.Error("Could not find matching file for submission Id {id}", submission.Id.Value);
            return;
        }

        // Upload file to Canvas
        // Include error checking/retry on failure
        var result = await _canvasGateway.UploadAssignmentSubmission(canvasCourseId, assignment.CanvasId, submission.StudentId, file);

        if (result)
            _logger.Information("Successfully uploaded submitted file for submission {@submission}", submission);
        else
        {
            _logger.Error("Error uploading file for submission {@submission}", submission);

            await _emailService.SendAssignmentUploadFailedNotification(assignment.Name, assignment.Id, submission.StudentId, submission.Id, cancellationToken);

            return;
        }
    }
}

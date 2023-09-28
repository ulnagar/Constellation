namespace Constellation.Application.Assignments.Events.AssignmentAttemptSubmittedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Assignments;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Shared;
using Core.DomainEvents;
using Core.Models;
using Core.Models.Subjects;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendUploadReceipt
    : IDomainEventHandler<AssignmentAttemptSubmittedDomainEvent>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public SendUploadReceipt(
        IAssignmentRepository assignmentRepository,
        ICourseRepository courseRepository,
        IStudentRepository studentRepository,
        IEmailService emailService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _assignmentRepository = assignmentRepository;
        _courseRepository = courseRepository;
        _studentRepository = studentRepository;
        _emailService = emailService;
        _dateTime = dateTime;
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
                .Warning("Failed to send Assignment Submission receipt to uploader");
            return;
        }

        if (assignment.DelayForwarding && assignment.ForwardingDate > _dateTime.Today)
        {
            _logger
                .ForContext(nameof(AssignmentAttemptSubmittedDomainEvent), notification, true)
                .ForContext(nameof(Error), new Error("UploadSubmissionToCanvas", "Assignment set to delay forwarding and delay date has not yet passed"), true)
                .Warning("Failed to send Assignment Submission receipt to uploader");

            return;
        }

        CanvasAssignmentSubmission submission = assignment.Submissions.FirstOrDefault(entry => entry.Id == notification.SubmissionId);

        if (submission is null)
        {
            _logger
                .ForContext(nameof(AssignmentAttemptSubmittedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.Assignments.Submission.NotFound(notification.SubmissionId), true)
                .Warning("Failed to send Assignment Submission receipt to uploader");

            return;
        }

        Course course = await _courseRepository.GetById(assignment.CourseId, cancellationToken);

        if (course is null)
        {
            _logger
                .ForContext(nameof(AssignmentAttemptSubmittedDomainEvent), notification, true)
                .ForContext(nameof(Error), CourseErrors.NotFound(assignment.CourseId), true)
                .Error("Failed to send Assignment Submission receipt to uploader");

            return;
        }

        Student student = await _studentRepository.GetById(submission.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(AssignmentAttemptSubmittedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Student.NotFound(submission.StudentId), true)
                .Error("Failed to send Assignment Submission receipt to uploader");

            return;
        }

        await _emailService.SendAssignmentUploadReceipt(assignment, submission, course, student, cancellationToken);
    }
}

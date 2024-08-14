namespace Constellation.Application.Assignments.Events.AssignmentAttemptSubmittedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Assignments;
using Constellation.Core.Models.Assignments.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Shared;
using Core.DomainEvents;
using Core.Models.Assignments.Errors;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.Students.Errors;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
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
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public SendUploadReceipt(
        IAssignmentRepository assignmentRepository,
        ICourseRepository courseRepository,
        IStudentRepository studentRepository,
        ISchoolContactRepository contactRepository,
        IEmailService emailService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _assignmentRepository = assignmentRepository;
        _courseRepository = courseRepository;
        _studentRepository = studentRepository;
        _contactRepository = contactRepository;
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
                .ForContext(nameof(Error), AssignmentErrors.NotFound(notification.AssignmentId), true)
                .Warning("Failed to send Assignment Submission receipt to uploader");
            return;
        }

        CanvasAssignmentSubmission submission = assignment.Submissions.FirstOrDefault(entry => entry.Id == notification.SubmissionId);

        if (submission is null)
        {
            _logger
                .ForContext(nameof(AssignmentAttemptSubmittedDomainEvent), notification, true)
                .ForContext(nameof(Error), SubmissionErrors.NotFound(notification.SubmissionId), true)
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
                .ForContext(nameof(Error), StudentErrors.NotFound(submission.StudentId), true)
                .Error("Failed to send Assignment Submission receipt to uploader");

            return;
        }

        SchoolContact contact = await _contactRepository.GetByName(submission.SubmittedBy, cancellationToken);

        if (contact is null)
        {
            _logger
                .ForContext(nameof(AssignmentAttemptSubmittedDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolContactErrors.NotFoundByName(submission.SubmittedBy), true)
                .Error("Failed to send Assignment Submission receipt to uploader");

            return;
        }

        await _emailService.SendAssignmentUploadReceipt(assignment, submission, course, student, contact, cancellationToken);
    }
}

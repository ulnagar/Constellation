namespace Constellation.Application.Domains.Assessments.Events.AssessmentSubmissionReceived;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Events;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Interfaces.Services;
using Serilog;

internal sealed class SendUploadReceipt
    : IDomainEventHandler<AssessmentSubmissionReceivedDomainEvent>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendUploadReceipt(
        IAssessmentRepository assessmentRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _emailService = emailService;
        _logger = logger
            .ForContext<AssessmentSubmissionReceivedDomainEvent>();
    }

    public async Task Handle(AssessmentSubmissionReceivedDomainEvent notification, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(notification.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(AssessmentSubmissionReceivedDomainEvent), notification, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(notification.AssessmentId), true)
                .Warning("Failed to send Assessment Submission receipt to uploader");
            return;
        }

        AssessmentStudent? student = assessment.Students.FirstOrDefault(entry => entry.Submissions.Any(submission => submission.Id == notification.SubmissionId));

        if (student is null)
        {
            _logger
                .ForContext(nameof(AssessmentSubmissionReceivedDomainEvent), notification, true)
                .ForContext(nameof(Error), AssessmentSubmissionErrors.NotFound(notification.SubmissionId), true)
                .Warning("Failed to send Assessment Submission receipt to uploader");

            return;
        }

        AssessmentSubmission submission = student.Submissions.First(entry => entry.Id == notification.SubmissionId);

        await _emailService.SendAssessmentSubmissionReceipt(assessment, student, submission, cancellationToken);
    }
}

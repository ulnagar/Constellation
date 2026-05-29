namespace Constellation.Application.Domains.Assessments.Assessments.Commands.SendAssessmentNotificationToSchools;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Models.Identifiers;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Services;
using Serilog;

internal sealed class SendAssessmentNotificationToSchoolsCommandHandler
: ICommandHandler<SendAssessmentNotificationToSchoolsCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendAssessmentNotificationToSchoolsCommandHandler(
        IAssessmentRepository assessmentRepository,
        ISchoolContactRepository contactRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _contactRepository = contactRepository;
        _emailService = emailService;
        _logger = logger
            .ForContext<SendAssessmentNotificationToSchoolsCommand>();
    }

    public async Task<Result> Handle(SendAssessmentNotificationToSchoolsCommand request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(SendAssessmentNotificationToSchoolsCommand), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to send Assessment Notification to school contacts");

            return Result.Failure(AssessmentErrors.NotFound(request.AssessmentId));
        }

        List<SchoolCode> schoolCodes = assessment.Students.Select(entry => entry.SchoolCode).Distinct().ToList();

        List<SchoolContact> contacts = await _contactRepository.GetBySchoolAndRole(schoolCodes, Position.Coordinator, cancellationToken);

        if (contacts.Count == 0)
        {
            _logger
                .ForContext(nameof(SendAssessmentNotificationToSchoolsCommand), request, true)
                .ForContext(nameof(Error), SchoolContactErrors.NoneFound, true)
                .Warning("Failed to send Assessment Notification to school contacts");

            return Result.Failure(SchoolContactErrors.NoneFound);
        }

        List<EmailRecipient> recipients = [];

        foreach (var contact in contacts)
        {
            var recipient = contact.GetEmailRecipient();

            if (recipient.IsFailure)
            {
                _logger
                    .ForContext(nameof(SendAssessmentNotificationToSchoolsCommand), request, true)
                    .ForContext(nameof(SchoolContact), contact, true)
                    .ForContext(nameof(Error), recipient.Error, true)
                    .Warning("Failed to include contact in Assessment Notifications");

                continue;
            }

            recipients.Add(recipient.Value);
        }

        recipients = recipients.Distinct().ToList();

        Dictionary<Result, List<EmailRecipient>> result = await _emailService.SendAssessmentNotificationToSchools(assessment, recipients, cancellationToken);

        List<KeyValuePair<Result, List<EmailRecipient>>> failedEmails = result
            .Where(entry => entry.Key.IsFailure)
            .ToList();

        if (failedEmails.Count > 0)
        {
            foreach (var group in failedEmails)
            {
                _logger
                    .ForContext(nameof(SendAssessmentNotificationToSchoolsCommand), request, true)
                    .ForContext(nameof(EmailRecipient), group.Value, true)
                    .ForContext(nameof(Error), group.Key.Error, true)
                    .Warning("Failed to send Assessment Notification to school contacts");
            }

            return Result.Failure(ApplicationErrors.UnknownError);
        }

        return Result.Success();
    }
}

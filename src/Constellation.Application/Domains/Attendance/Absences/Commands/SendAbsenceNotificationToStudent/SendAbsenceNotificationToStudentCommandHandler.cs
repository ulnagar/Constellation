namespace Constellation.Application.Domains.Attendance.Absences.Commands.SendAbsenceNotificationToStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Absences.Identifiers;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Identifiers;
using Constellation.Core.Models.Tutorials.Repositories;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using ConvertAbsenceToAbsenceEntry;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendAbsenceNotificationToStudentCommandHandler
    : ICommandHandler<SendAbsenceNotificationToStudentCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public SendAbsenceNotificationToStudentCommandHandler(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IOfferingRepository offeringRepository,
        ITutorialRepository tutorialRepository,
        IEmailService emailService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
        _tutorialRepository = tutorialRepository;
        _emailService = emailService;
        _dateTime = dateTime;
        _logger = logger.ForContext<SendAbsenceNotificationToStudentCommand>();
    }

    public async Task<Result> Handle(SendAbsenceNotificationToStudentCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("{jobId}: Could not find student with Id {studentId}", request.JobId, request.StudentId);

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        List<Absence> absences = new();
        foreach (AbsenceId absenceId in request.AbsenceIds)
        {
            Absence absence = await _absenceRepository.GetById(absenceId, cancellationToken);

            if (absence is not null && !absence.Explained)
                absences.Add(absence);
        }

        if (absences.Count == 0)
        {
            _logger
                .ForContext(nameof(request.AbsenceIds), request.AbsenceIds)
                .Warning("{jobId}: Could not find any valid absences from Ids provided", request.JobId);

            return Result.Failure(DomainErrors.Absences.Absence.NotFound(request.AbsenceIds.First()));
        }

        List<EmailRecipient> recipients = new();

        Result<EmailRecipient> result = EmailRecipient.Create(student.Name.DisplayName, student.EmailAddress.Email);

        if (result.IsSuccess)
        {
            recipients.Add(result.Value);
        }

        List<AbsenceEntry> absenceEntries = new();

        foreach (Absence absence in absences)
        {
            string activityName = string.Empty;

            if (absence.Source == AbsenceSource.Offering)
            {
                OfferingId offeringId = OfferingId.FromValue(absence.SourceId);

                Offering offering = await _offeringRepository.GetById(offeringId, cancellationToken);

                if (offering is null)
                {
                    _logger.Warning("Could not find offering with Id {id}", offeringId);

                    continue;
                }

                activityName = offering.Name;
            }

            if (absence.Source == AbsenceSource.Tutorial)
            {
                TutorialId tutorialId = TutorialId.FromValue(absence.SourceId);

                Tutorial tutorial = await _tutorialRepository.GetById(tutorialId, cancellationToken);

                if (tutorial is null)
                {
                    _logger.Warning("Could not find tutorial with Id {id}", tutorialId);

                    continue;
                }

                activityName = tutorial.Name;
            }

            absenceEntries.Add(new(
                absence.Id,
                absence.Date,
                absence.PeriodName,
                absence.PeriodTimeframe,
                activityName,
                absence.AbsenceTimeframe,
                absence.AbsenceLength));
        }

        Result<EmailDtos.SentEmail> sentEmail = await _emailService.SendStudentPartialAbsenceExplanationRequest(absenceEntries, student, recipients, cancellationToken);

        if (sentEmail.IsFailure)
            return sentEmail;

        foreach (Absence absence in absences)
        {
            absence.AddNotification(NotificationType.Email, sentEmail.Value.message, student.EmailAddress.Email, sentEmail.Value.id, _dateTime.Now);

            foreach (EmailRecipient recipient in recipients)
                _logger.Information("{id}: Message sent via Email to {student} ({email}) for Partial Absence on {Date}", request.JobId, recipient.Name, recipient.Email, absence.Date.ToShortDateString());
        }

        return Result.Success();
    }
}

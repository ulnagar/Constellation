namespace Constellation.Application.Absences.SendAbsenceNotificationToStudent;

using Constellation.Application.Absences.ConvertAbsenceToAbsenceEntry;
using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Core.Abstractions.Clock;
using Core.Errors;
using Core.Models.Students.Errors;
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
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public SendAbsenceNotificationToStudentCommandHandler(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IOfferingRepository offeringRepository,
        IEmailService emailService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _offeringRepository = offeringRepository;
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

        Result<EmailRecipient> result = EmailRecipient.Create(student.DisplayName, student.EmailAddress);

        if (result.IsSuccess)
        {
            recipients.Add(result.Value);
        }

        List<AbsenceEntry> absenceEntries = new();

        foreach (Absence absence in absences)
        {
            Offering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

            if (offering is null)
                continue;

            absenceEntries.Add(new(
                absence.Id,
                absence.Date,
                absence.PeriodName,
                absence.PeriodTimeframe,
                offering.Name,
                absence.AbsenceTimeframe,
                absence.AbsenceLength));
        }

        EmailDtos.SentEmail sentEmail = await _emailService.SendStudentPartialAbsenceExplanationRequest(absenceEntries, student, recipients, cancellationToken);

        if (sentEmail is null)
        {
            return Result.Failure(new("Gateway.Email", "Failed to send email"));
        }

        foreach (Absence absence in absences)
        {
            absence.AddNotification(NotificationType.Email, sentEmail.message, student.EmailAddress, sentEmail.id, _dateTime.Now);

            foreach (EmailRecipient recipient in recipients)
                _logger.Information("{id}: Message sent via Email to {student} ({email}) for Partial Absence on {Date}", request.JobId, recipient.Name, recipient.Email, absence.Date.ToShortDateString());
        }

        return Result.Success();
    }
}

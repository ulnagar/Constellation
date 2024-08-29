namespace Constellation.Application.Absences.SendAbsenceDigestToStudent;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using ConvertAbsenceToAbsenceEntry;
using Core.Abstractions.Clock;
using Core.Models.Offerings;
using Core.Shared;
using Core.ValueObjects;
using DTOs;
using Interfaces.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendAbsenceDigestToStudentCommandHandler
: ICommandHandler<SendAbsenceDigestToStudentCommand>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    private readonly List<Offering> _cachedOfferings = new();

    public SendAbsenceDigestToStudentCommandHandler(
        IOfferingRepository offeringRepository,
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IEmailService emailService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _emailService = emailService;
        _dateTime = dateTime;
        _logger = logger.ForContext<SendAbsenceDigestToStudentCommand>();
    }

    public async Task<Result> Handle(SendAbsenceDigestToStudentCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(SendAbsenceDigestToStudentCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(request.StudentId), true)
                .Warning("Failed to send Absence Digest to Student");

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        List<Absence> digestPartialAbsences = await _absenceRepository.GetUnexplainedPartialAbsencesForStudentWithDelay(student.Id, 1, cancellationToken);

        if (!digestPartialAbsences.Any()) 
            return Result.Success();

        List<EmailRecipient> recipients = new();
            
        Result<EmailRecipient> result = EmailRecipient.Create(student.Name.DisplayName, student.EmailAddress.Email);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Student), student, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to send Absence Digest to Student");

            return Result.Failure(result.Error);
        }

        recipients.Add(result.Value);

        List<AbsenceEntry> partialAbsenceEntries = await ProcessAbsences(digestPartialAbsences, cancellationToken);

        if (!partialAbsenceEntries.Any())
            return Result.Success();

        EmailDtos.SentEmail sentMessage = await _emailService.SendStudentAbsenceDigest(partialAbsenceEntries, student, recipients, cancellationToken);

        if (sentMessage is null)
            return Result.Failure(new("Gateway.Email", "Failed to send email"));

        UpdateAbsenceWithNotification(request.JobId, digestPartialAbsences, sentMessage, recipients, student);

        return Result.Success();
    }

    private void UpdateAbsenceWithNotification(
        Guid jobId,
        List<Absence> absences,
        EmailDtos.SentEmail sentMessage,
        List<EmailRecipient> recipients,
        Student student)
    {
        string emails = string.Join(", ", recipients.Select(entry => entry.Email));

        foreach (Absence absence in absences)
        {
            absence.AddNotification(
                NotificationType.Email,
                sentMessage.message,
                emails,
                sentMessage.id,
                _dateTime.Now);
        }
    }

    private async Task<List<AbsenceEntry>> ProcessAbsences(List<Absence> absences, CancellationToken cancellationToken)
    {
        List<AbsenceEntry> response = new();

        foreach (Absence absence in absences)
        {
            Offering offering = _cachedOfferings.FirstOrDefault(offering => offering.Id == absence.OfferingId);

            if (offering is null)
            {
                offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

                if (offering is null)
                    continue;

                _cachedOfferings.Add(offering);
            }

            response.Add(new(
                absence.Id,
                absence.Date,
                absence.PeriodName,
                absence.PeriodTimeframe,
                offering.Name,
                absence.AbsenceTimeframe,
                absence.AbsenceLength));
        }

        return response;
    }
}
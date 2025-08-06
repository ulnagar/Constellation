namespace Constellation.Application.Domains.Attendance.Absences.Commands.SendAbsenceDigestToStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Tutorials.Identifiers;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using ConvertAbsenceToAbsenceEntry;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Repositories;
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
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    private readonly List<Offering> _cachedOfferings = new();
    private readonly List<Tutorial> _cachedTutorials = new();

    public SendAbsenceDigestToStudentCommandHandler(
        IOfferingRepository offeringRepository,
        ITutorialRepository tutorialRepository,
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IEmailService emailService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _tutorialRepository = tutorialRepository;
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

        Result<EmailDtos.SentEmail> sentMessage = await _emailService.SendStudentAbsenceDigest(partialAbsenceEntries, student, recipients, cancellationToken);

        if (sentMessage.IsFailure)
            return sentMessage;

        UpdateAbsenceWithNotification(request.JobId, digestPartialAbsences, sentMessage.Value, recipients, student);

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
            string activityName = string.Empty;

            if (absence.Source == AbsenceSource.Offering)
            {
                OfferingId offeringId = OfferingId.FromValue(absence.SourceId);

                Offering offering = _cachedOfferings.FirstOrDefault(offering => offering.Id == offeringId);

                if (offering is null)
                {
                    offering = await _offeringRepository.GetById(offeringId, cancellationToken);

                    if (offering is null)
                        continue;

                    _cachedOfferings.Add(offering);
                }

                activityName = offering.Name;
            }

            if (absence.Source == AbsenceSource.Tutorial)
            {
                TutorialId tutorialId = TutorialId.FromValue(absence.SourceId);

                Tutorial tutorial = _cachedTutorials.FirstOrDefault(tutorial => tutorial.Id == tutorialId);

                if (tutorial is null)
                {
                    tutorial = await _tutorialRepository.GetById(tutorialId, cancellationToken);

                    if (tutorial is null)
                        continue;

                    _cachedTutorials.Add(tutorial);
                }

                activityName = tutorial.Name;
            }

            response.Add(new(
                absence.Id,
                absence.Date,
                absence.PeriodName,
                absence.PeriodTimeframe,
                activityName,
                absence.AbsenceTimeframe,
                absence.AbsenceLength));
        }

        return response;
    }
}
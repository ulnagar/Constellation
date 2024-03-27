namespace Constellation.Application.Absences.SendAbsenceDigestToCoordinator;

using ConvertAbsenceToAbsenceEntry;
using Abstractions.Messaging;
using DTOs;
using Interfaces.Repositories;
using Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.Students;
using Core.Shared;
using Core.ValueObjects;
using Core.Abstractions.Clock;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.Students.Errors;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Constellation.Core.Models.Students.Repositories;

internal sealed class SendAbsenceDigestToCoordinatorCommandHandler
    : ICommandHandler<SendAbsenceDigestToCoordinatorCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly ISchoolContactRepository _schoolContactRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;
    private readonly List<Offering> _cachedOfferings = new();

    public SendAbsenceDigestToCoordinatorCommandHandler(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        ISchoolContactRepository schoolContactRepository,
        IOfferingRepository offeringRepository,
        ISchoolRepository schoolRepository,
        IEmailService emailService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _schoolContactRepository = schoolContactRepository;
        _offeringRepository = offeringRepository;
        _schoolRepository = schoolRepository;
        _emailService = emailService;
        _dateTime = dateTime;
        _logger = logger.ForContext<SendAbsenceDigestToCoordinatorCommand>();
    }

    public async Task<Result> Handle(SendAbsenceDigestToCoordinatorCommand request, CancellationToken cancellationToken) 
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("{jobId}: Could not find student with Id {studentId}", request.JobId, request.StudentId);

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        List<Absence> digestWholeAbsences = await _absenceRepository.GetUnexplainedWholeAbsencesForStudentWithDelay(student.StudentId, 2, cancellationToken);
        List<Absence> digestPartialAbsences = await _absenceRepository.GetUnverifiedPartialAbsencesForStudentWithDelay(student.StudentId, 1, cancellationToken);

        if (digestWholeAbsences.Any() || digestPartialAbsences.Any())
        {
            List<SchoolContact> coordinators = await _schoolContactRepository.GetBySchoolAndRole(student.SchoolCode, SchoolContactRole.Coordinator, cancellationToken);

            List<EmailRecipient> recipients = new();

            foreach (SchoolContact coordinator in coordinators)
            {
                Result<Name> nameResult = Name.Create(coordinator.FirstName, string.Empty, coordinator.LastName);
                if (nameResult.IsFailure)
                    continue;

                Result<EmailRecipient> result = EmailRecipient.Create(nameResult.Value.DisplayName, coordinator.EmailAddress);

                if (result.IsSuccess && recipients.All(recipient => result.Value.Email != recipient.Email))
                    recipients.Add(result.Value);
            }

            School school = await _schoolRepository.GetById(student.SchoolCode, cancellationToken);

            if (school is null)
            {
                _logger.Warning("{jobId}: Could not find School with Id {id}", request.JobId, student.SchoolCode);

                return Result.Failure(DomainErrors.Partners.School.NotFound(student.SchoolCode));
            }

            if (!recipients.Any()) 
            {
                Result<EmailRecipient> result = EmailRecipient.Create(school.Name, school.EmailAddress);

                if (result.IsFailure)
                {
                    _logger.Warning("{jobId}: No recipients could be found or created: {school}", request.JobId, school);

                    return Result.Failure(DomainErrors.Partners.Contact.NotFound(0));
                }

                recipients.Add(result.Value);
            }

            List<AbsenceEntry> wholeAbsenceEntries = await ProcessAbsences(digestWholeAbsences, cancellationToken);
            List<AbsenceEntry> partialAbsenceEntries = await ProcessAbsences(digestPartialAbsences, cancellationToken);

            if (!wholeAbsenceEntries.Any() && !partialAbsenceEntries.Any())
                return Result.Success();

            EmailDtos.SentEmail sentMessage = await _emailService.SendCoordinatorAbsenceDigest(wholeAbsenceEntries, partialAbsenceEntries, student, school, recipients, cancellationToken);

            if (sentMessage is null)
            {
                _logger.Warning("{jobId}: Email failed to send: {@message}", request.JobId, sentMessage);

                return Result.Failure(new("MailSender", "Unknown Error"));
            }

            UpdateAbsenceWithNotification(request.JobId, digestWholeAbsences.Concat(digestPartialAbsences).ToList(), sentMessage, recipients, student);
        }

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

            foreach (EmailRecipient recipient in recipients)
                _logger
                    .ForContext("JobId", jobId)
                    .ForContext(nameof(Student), student, true)
                    .Information("{id}: School digest sent to {recipient} for {student}", jobId, recipient.Name, student.DisplayName);
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

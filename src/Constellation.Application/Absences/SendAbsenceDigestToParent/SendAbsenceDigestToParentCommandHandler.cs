namespace Constellation.Application.Absences.SendAbsenceDigestToParent;

using Constellation.Application.Absences.ConvertAbsenceToAbsenceEntry;
using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Core.Abstractions.Clock;
using Core.Models.Students.Errors;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendAbsenceDigestToParentCommandHandler
    : ICommandHandler<SendAbsenceDigestToParentCommand>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;
    private readonly List<Offering> _cachedOfferings = new();

    public SendAbsenceDigestToParentCommandHandler(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IFamilyRepository familyRepository,
        IOfferingRepository offeringRepository,
        IEmailService emailService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _familyRepository = familyRepository;
        _offeringRepository = offeringRepository;
        _emailService = emailService;
        _dateTime = dateTime;
        _logger = logger.ForContext<SendAbsenceDigestToParentCommand>();
    }

    public async Task<Result> Handle(SendAbsenceDigestToParentCommand request, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("{jobId}: Could not find student with Id {studentId}", request.JobId, request.StudentId);

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        List<Absence> digestWholeAbsences = await _absenceRepository.GetUnexplainedWholeAbsencesForStudentWithDelay(student.StudentId, 1, cancellationToken);
        List<Absence> digestPartialAbsences = await _absenceRepository.GetUnexplainedPartialAbsencesForStudentWithDelay(student.StudentId, 1, cancellationToken);

        if (digestWholeAbsences.Any() || digestPartialAbsences.Any())
        {
            List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.StudentId, cancellationToken);
            List<Parent> parents = families.SelectMany(family => family.Parents).ToList();
            List<EmailRecipient> recipients = new();

            foreach (Family family in families)
            {
                Result<EmailRecipient> result = EmailRecipient.Create(family.FamilyTitle, family.FamilyEmail);

                if (result.IsSuccess)
                    recipients.Add(result.Value);
            }

            foreach (Parent parent in parents)
            {
                Result<Name> nameResult = Name.Create(parent.FirstName, string.Empty, parent.LastName);

                if (nameResult.IsFailure)
                    continue;

                Result<EmailRecipient> result = EmailRecipient.Create(nameResult.Value.DisplayName, parent.EmailAddress);

                if (result.IsSuccess && recipients.All(recipient => result.Value.Email != recipient.Email))
                    recipients.Add(result.Value);
            }

            if (recipients.Any())
            {
                List<AbsenceEntry> wholeAbsenceEntries = await ProcessAbsences(digestWholeAbsences, cancellationToken);
                List<AbsenceEntry> partialAbsenceEntries = await ProcessAbsences(digestPartialAbsences, cancellationToken);

                if (!wholeAbsenceEntries.Any() && !partialAbsenceEntries.Any()) 
                    return Result.Success();

                EmailDtos.SentEmail sentMessage = await _emailService.SendParentAbsenceDigest(wholeAbsenceEntries, partialAbsenceEntries, student, recipients, cancellationToken);

                if (sentMessage is null)
                    return Result.Failure(new("Gateway.Email", "Failed to send email"));

                UpdateAbsenceWithNotification(request.JobId, digestWholeAbsences.Concat(digestPartialAbsences).ToList(), sentMessage, recipients, student);
            }
            else
            {
                await _emailService.SendAdminAbsenceContactAlert(student.DisplayName);
            }
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

                    .Information("{id}: Parent digest sent to {address} for {student}", jobId, recipient.Email, student.DisplayName);
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

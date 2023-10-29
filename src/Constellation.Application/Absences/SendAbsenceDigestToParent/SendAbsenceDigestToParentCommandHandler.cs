namespace Constellation.Application.Absences.SendAbsenceDigestToParent;

using Constellation.Application.Absences.ConvertAbsenceToAbsenceEntry;
using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Core.Models.Students.Errors;
using Serilog;
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
    private readonly ILogger _logger;

    public SendAbsenceDigestToParentCommandHandler(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IFamilyRepository familyRepository,
        IOfferingRepository offeringRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _familyRepository = familyRepository;
        _offeringRepository = offeringRepository;
        _emailService = emailService;
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

        List<Absence> digestAbsences = await _absenceRepository.GetUnexplainedWholeAbsencesForStudentWithDelay(student.StudentId, 1, cancellationToken);

        if (digestAbsences.Any())
        {
            List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.StudentId, cancellationToken);
            List<Parent> parents = families.SelectMany(family => family.Parents).ToList();
            List<EmailRecipient> recipients = new();

            foreach (var family in families)
            {
                Result<EmailRecipient> result = EmailRecipient.Create(family.FamilyTitle, family.FamilyEmail);

                if (result.IsSuccess)
                    recipients.Add(result.Value);
            }

            foreach (var parent in parents)
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
                List<AbsenceEntry> absenceEntries = new();

                foreach (Absence absence in digestAbsences)
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
                        absence.AbsenceTimeframe));
                }

                EmailDtos.SentEmail sentmessage = await _emailService.SendParentWholeAbsenceDigest(absenceEntries, student, recipients, cancellationToken);

                if (sentmessage == null)
                    return Result.Failure(Error.None);

                string emails = string.Join(", ", recipients.Select(entry => entry.Email));

                foreach (Absence absence in digestAbsences)
                {
                    absence.AddNotification(
                    NotificationType.Email,
                        sentmessage.message,
                        emails);

                    foreach (var recipient in recipients)
                        _logger.Information("{id}: Parent digest sent to {address} for {student}", request.JobId, recipient.Email, student.DisplayName);
                }

                absenceEntries = null;
            }
            else
            {
                await _emailService.SendAdminAbsenceContactAlert(student.DisplayName);
            }

            families = null;
            parents = null;
            recipients = null;
            digestAbsences = null;
        }

        return Result.Success();
    }
}

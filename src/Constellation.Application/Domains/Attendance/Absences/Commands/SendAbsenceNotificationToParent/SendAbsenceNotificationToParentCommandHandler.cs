namespace Constellation.Application.Domains.Attendance.Absences.Commands.SendAbsenceNotificationToParent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Absences.Identifiers;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using ConvertAbsenceToAbsenceEntry;
using Core.Models.Messaging.Email;
using Core.Models.Offerings.Identifiers;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.Repositories;
using Messaging.Sms.Dtos;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendAbsenceNotificationToParentCommandHandler
    : ICommandHandler<SendAbsenceNotificationToParentCommand>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ISMSService _smsService;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public SendAbsenceNotificationToParentCommandHandler(
        IAbsenceRepository absenceRepository,
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        IOfferingRepository offeringRepository,
        ITutorialRepository tutorialRepository,
        ISMSService smsService,
        IEmailService emailService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _offeringRepository = offeringRepository;
        _tutorialRepository = tutorialRepository;
        _smsService = smsService;
        _emailService = emailService;
        _dateTime = dateTime;
        _logger = logger.ForContext<SendAbsenceNotificationToParentCommand>();
    }

    public async Task<Result> Handle(SendAbsenceNotificationToParentCommand request, CancellationToken cancellationToken)
    {
        if (request.AbsenceIds.Count == 0)
        {
            _logger.Warning("{jobId}: No absences defined to send notifications for.", request.JobId);
            return Result.Failure(IntegrationErrors.Absences.Notifications.Parents.NoAbsencesDetected);
        }

        Student? student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("{jobId}: Could not find student with Id {studentId}", request.JobId, request.StudentId);

            return Result.Failure(StudentErrors.NotFound(request.StudentId));
        }

        List<Absence> absences = [];
        foreach (AbsenceId absenceId in request.AbsenceIds)
        {
            Absence? absence = await _absenceRepository.GetById(absenceId, cancellationToken);

            if (absence is not null && !absence.Explained)
                absences.Add(absence);
        }

        if (absences.Count == 0)
        {
            _logger
                .ForContext(nameof(request.AbsenceIds), request.AbsenceIds)
                .Warning("{jobId}: Could not find any valid absences from Ids provided", request.JobId);

            return Result.Failure(IntegrationErrors.Absences.Notifications.Parents.NoAbsencesDetected);
        }

        List<AbsenceEntry> absenceEntries = [];

        foreach (Absence absence in absences)
        {
            string activityName = string.Empty;

            if (absence.Source == AbsenceSource.Offering)
            {
                OfferingId offeringId = OfferingId.FromValue(absence.SourceId);

                Offering? offering = await _offeringRepository.GetById(offeringId, cancellationToken);

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

                Tutorial? tutorial = await _tutorialRepository.GetById(tutorialId, cancellationToken);

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

        List<IGrouping<DateOnly, AbsenceEntry>> groupedAbsences = absenceEntries.GroupBy(absence => absence.Date).ToList();

        List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.Id, cancellationToken);

        foreach (Family family in families)
        {
            List<EmailRecipient> recipients = [];
            List<PhoneNumber> phoneNumbers = [];

            Result<EmailRecipient> familyEmail = EmailRecipient.Create(family.FamilyTitle, family.FamilyEmail);

            if (familyEmail.IsSuccess)
                recipients.Add(familyEmail.Value);

            List<PhoneNumber> numbers = family.Parents
                .Select(parent => parent.MobileNumber)
                .Distinct()
                .ToList();

            foreach (PhoneNumber number in numbers)
                phoneNumbers.Add(number);

            foreach (Parent parent in family.Parents)
            {
                Result<EmailRecipient> result =
                    EmailRecipient.Create(parent.Name, parent.EmailAddress);

                if (result.IsSuccess && recipients.All(recipient => result.Value.Email != recipient.Email))
                    recipients.Add(result.Value);
            }

            foreach (IGrouping<DateOnly, AbsenceEntry> group in groupedAbsences)
            {
                if (phoneNumbers.Count > 0 && group.Key == DateOnly.FromDateTime(DateTime.Today.AddDays(-1)))
                {
                    Result<List<OutgoingSmsConfirmation>> sentMessages = await _smsService.SendAbsenceNotification(
                        group.ToList(),
                        student,
                        phoneNumbers,
                        cancellationToken);

                    if (sentMessages.IsFailure)
                    {
                        // SMS Gateway failed. Send via email instead.
                        _logger.Warning("{id}: SMS Sending Failed! Fallback to Email notifications.", request.JobId);

                        if (recipients.Count > 0)
                        {
                            Result<EmailMessage> message = await _emailService.SendParentWholeAbsenceAlert(
                                family.FamilyTitle, 
                                group.ToList(), 
                                student, 
                                recipients,
                                cancellationToken);

                            if (message.IsFailure)
                            {
                                _logger.Error("{id}: Email Sending Failed! No further fallback possible!", request.JobId);
                                continue;
                            }

                            foreach (AbsenceEntry entry in group)
                            {
                                string emails = string.Join(", ", recipients.Select(recipient => recipient.Email));
                                Absence absence = absences.First(absence => absence.Id == entry.Id);

                                absence.AddNotification(NotificationType.Email, message.Value.BodyText, emails, message.Value.Id.ToString(),
                                    _dateTime.Now);

                                foreach (EmailRecipient recipient in recipients)
                                    _logger.Information(
                                        "{id}: Message sent via Email to {email} for Whole Absence on {Date}",
                                        request.JobId, recipient.Email, group.Key.ToShortDateString());
                            }
                        }
                        else
                        {
                            _logger.Error("{id}: Email addresses not found! Parents have not been notified!",
                                request.JobId);
                        }
                    }
                    else
                    {
                        // Once the message has been sent, add it to the database.
                        if (sentMessages.Value.Count == 0)
                            continue;

                        foreach (AbsenceEntry entry in group)
                        {
                            foreach (OutgoingSmsConfirmation confirmation in sentMessages.Value)
                            {
                                Absence absence = absences.First(absence => absence.Id == entry.Id);

                                absence.AddNotification(
                                    NotificationType.SMS, 
                                    confirmation.Message ?? string.Empty,
                                    confirmation.Destination ?? string.Empty, 
                                    confirmation.OutgoingId ?? string.Empty, 
                                    _dateTime.Now);

                                _logger.Information(
                                    "{id}: Message sent via SMS to {number} for Whole Absence on {Date}", 
                                    request.JobId,
                                    confirmation.Destination, 
                                    group.Key.ToShortDateString());
                            }
                        }
                    }
                }
                else if (recipients.Count > 0)
                {
                    Result<EmailMessage> message = await _emailService.SendParentWholeAbsenceAlert(
                        family.FamilyTitle, 
                        group.ToList(), 
                        student, 
                        recipients,
                        cancellationToken);

                    if (message.IsFailure)
                    {
                        _logger.Error("{id}: Email Sending Failed! No further fallback possible!", request.JobId);
                        continue;
                    }

                    foreach (AbsenceEntry entry in group)
                    {
                        string emails = string.Join(", ", recipients.Select(recipient => recipient.Email));
                        Absence absence = absences.First(absence => absence.Id == entry.Id);

                        absence.AddNotification(NotificationType.Email, message.Value.BodyText, emails, message.Value.Id.ToString(),
                            _dateTime.Now);

                        foreach (EmailRecipient recipient in recipients)
                            _logger.Information("{id}: Message sent via Email to {email} for Whole Absence on {Date}",
                                request.JobId, recipient.Email, group.Key.ToShortDateString());
                    }
                }
                else
                {
                    await _emailService.SendAdminAbsenceContactAlert(student.Name.DisplayName);
                }
            }
        }

        return Result.Success();
    }
}

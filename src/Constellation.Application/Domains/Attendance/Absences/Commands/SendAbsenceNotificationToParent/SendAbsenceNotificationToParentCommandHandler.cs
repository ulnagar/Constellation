namespace Constellation.Application.Domains.Attendance.Absences.Commands.SendAbsenceNotificationToParent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using ConvertAbsenceToAbsenceEntry;
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
    private readonly ISMSService _smsService;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public SendAbsenceNotificationToParentCommandHandler(
        IAbsenceRepository absenceRepository,
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        IOfferingRepository offeringRepository,
        ISMSService smsService,
        IEmailService emailService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _offeringRepository = offeringRepository;
        _smsService = smsService;
        _emailService = emailService;
        _dateTime = dateTime;
        _logger = logger.ForContext<SendAbsenceNotificationToParentCommand>();
    }

    public async Task<Result> Handle(SendAbsenceNotificationToParentCommand request, CancellationToken cancellationToken)
    {
        if (!request.AbsenceIds.Any())
        {
            _logger.Warning("{jobId}: No absences defined to send notifications for.", request.JobId);
            return Result.Failure(IntegrationErrors.Absences.Notifications.Parents.NoAbsencesDetected);
        }

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

            return Result.Failure(IntegrationErrors.Absences.Notifications.Parents.NoAbsencesDetected);
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

        List<IGrouping<DateOnly, AbsenceEntry>> groupedAbsences = absenceEntries.GroupBy(absence => absence.Date).ToList();

        List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.Id, cancellationToken);

        foreach (Family family in families)
        {
            List<EmailRecipient> recipients = new();
            List<PhoneNumber> phoneNumbers = new();

            Result<EmailRecipient> familyEmail = EmailRecipient.Create(family.FamilyTitle, family.FamilyEmail);

            if (familyEmail.IsSuccess)
                recipients.Add(familyEmail.Value);

            List<string> numbers = family.Parents
                .Select(parent => parent.MobileNumber)
                .Distinct()
                .ToList();

            foreach (string number in numbers)
            {
                Result<PhoneNumber> result = PhoneNumber.Create(number);

                if (result.IsSuccess)
                    phoneNumbers.Add(result.Value);
            }

            foreach (Parent parent in family.Parents)
            {
                Result<Name> nameResult = Name.Create(parent.FirstName, string.Empty, parent.LastName);

                if (nameResult.IsFailure)
                    continue;

                Result<EmailRecipient> result =
                    EmailRecipient.Create(nameResult.Value.DisplayName, parent.EmailAddress);

                if (result.IsSuccess && recipients.All(recipient => result.Value.Email != recipient.Email))
                    recipients.Add(result.Value);
            }

            foreach (IGrouping<DateOnly, AbsenceEntry> group in groupedAbsences)
            {
                if (phoneNumbers.Any() && group.Key == DateOnly.FromDateTime(DateTime.Today.AddDays(-1)))
                {
                    Result<SMSMessageCollectionDto> sentMessages = await _smsService.SendAbsenceNotification(
                        group.ToList(),
                        student,
                        phoneNumbers,
                        cancellationToken);

                    if (sentMessages.IsFailure)
                    {
                        // SMS Gateway failed. Send via email instead.
                        _logger.Warning("{id}: SMS Sending Failed! Fallback to Email notifications.", request.JobId);

                        if (recipients.Any())
                        {
                            Result<EmailDtos.SentEmail> message = await _emailService.SendParentWholeAbsenceAlert(
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
                                string emails = string.Join(", ", recipients.Select(entry => entry.Email));
                                Absence absence = absences.First(absence => absence.Id == entry.Id);

                                absence.AddNotification(NotificationType.Email, message.Value.message, emails, message.Value.id,
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

                    // Once the message has been sent, add it to the database.
                    if (sentMessages.Value.Messages.Count > 0)
                    {
                        foreach (AbsenceEntry entry in group)
                        {
                            string sentToNumbers = string.Join(", ",
                                phoneNumbers.Select(entry => entry.ToString(PhoneNumber.Format.Mobile)));
                            Absence absence = absences.First(absence => absence.Id == entry.Id);

                            absence.AddNotification(NotificationType.SMS, sentMessages.Value.Messages.First().MessageBody,
                                sentToNumbers, sentMessages.Value.Messages.First().OutgoingId, _dateTime.Now);

                            foreach (PhoneNumber number in phoneNumbers)
                                _logger.Information(
                                    "{id}: Message sent via SMS to {number} for Whole Absence on {Date}", request.JobId,
                                    number.ToString(PhoneNumber.Format.Mobile), group.Key.ToShortDateString());
                        }
                    }
                }
                else if (recipients.Any())
                {
                    Result<EmailDtos.SentEmail> message = await _emailService.SendParentWholeAbsenceAlert(
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
                        string emails = string.Join(", ", recipients.Select(entry => entry.Email));
                        Absence absence = absences.First(absence => absence.Id == entry.Id);

                        absence.AddNotification(NotificationType.Email, message.Value.message, emails, message.Value.id,
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

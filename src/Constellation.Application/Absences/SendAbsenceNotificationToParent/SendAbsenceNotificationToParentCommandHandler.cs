namespace Constellation.Application.Absences.SendAbsenceNotificationToParent;

using Constellation.Application.Absences.ConvertAbsenceToAbsenceEntry;
using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
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
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly ISMSService _smsService;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendAbsenceNotificationToParentCommandHandler(
        IAbsenceRepository absenceRepository,
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        ICourseOfferingRepository offeringRepository,
        ISMSService smsService,
        IEmailService emailService,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _offeringRepository = offeringRepository;
        _smsService = smsService;
        _emailService = emailService;
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

            return Result.Failure(DomainErrors.Partners.Student.NotFound(request.StudentId));
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

        List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.StudentId, cancellationToken);
        List<Parent> parents = families.SelectMany(family => family.Parents).ToList();

        List<string> numbers = parents
            .Select(parent => parent.MobileNumber)
            .Distinct()
            .ToList();

        List<PhoneNumber> phoneNumbers = new();

        foreach (string number in numbers)
        {
            Result<PhoneNumber> result = PhoneNumber.Create(number);

            if (result.IsSuccess)
                phoneNumbers.Add(result.Value);
        }

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

        List<AbsenceEntry> absenceEntries = new();

        foreach (Absence absence in absences)
        {
            CourseOffering offering = await _offeringRepository.GetById(absence.OfferingId, cancellationToken);

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

        List<IGrouping<DateOnly, AbsenceEntry>> groupedAbsences = absenceEntries.GroupBy(absence => absence.Date).ToList();

        foreach (IGrouping<DateOnly, AbsenceEntry> group in groupedAbsences)
        {
            if (phoneNumbers.Any() && group.Key == DateOnly.FromDateTime(DateTime.Today.AddDays(-1)))
            {
                SMSMessageCollectionDto sentMessages = await _smsService.SendAbsenceNotification(
                    group.ToList(),
                    student,
                    phoneNumbers,
                    cancellationToken);

                if (sentMessages == null)
                {
                    // SMS Gateway failed. Send via email instead.
                    _logger.Warning("{id}: SMS Sending Failed! Fallback to Email notifications.", request.JobId);

                    if (recipients.Any())
                    {
                        EmailDtos.SentEmail message = await _emailService.SendParentWholeAbsenceAlert(group.ToList(), student, recipients, cancellationToken);

                        foreach (AbsenceEntry entry in group)
                        {
                            string emails = string.Join(", ", recipients.Select(entry => entry.Email));
                            Absence absence = absences.First(absence => absence.Id == entry.Id);

                            absence.AddNotification(NotificationType.Email, message.message, emails);

                            foreach (EmailRecipient recipient in recipients)
                                _logger.Information("{id}: Message sent via Email to {email} for Whole Absence on {Date}", request.JobId, recipient.Email, group.Key.ToShortDateString());
                        }
                    }
                    else
                    {
                        _logger.Error("{id}: Email addresses not found! Parents have not been notified!", request.JobId);
                    }
                }

                // Once the message has been sent, add it to the database.
                if (sentMessages.Messages.Count > 0)
                {
                    foreach (AbsenceEntry entry in group)
                    {
                        string sentToNumbers = string.Join(", ", phoneNumbers.Select(entry => entry.ToString(PhoneNumber.Format.Mobile)));
                        Absence absence = absences.First(absence => absence.Id == entry.Id);

                        absence.AddNotification(NotificationType.SMS, sentMessages.Messages.First().MessageBody, sentToNumbers);

                        foreach (PhoneNumber number in phoneNumbers)
                            _logger.Information("{id}: Message sent via SMS to {number} for Whole Absence on {Date}", request.JobId, number.ToString(PhoneNumber.Format.Mobile), group.Key.ToShortDateString());
                    }
                }
            }
            else if (recipients.Any())
            {
                EmailDtos.SentEmail message = await _emailService.SendParentWholeAbsenceAlert(group.ToList(), student, recipients, cancellationToken);

                foreach (AbsenceEntry entry in group)
                {
                    string emails = string.Join(", ", recipients.Select(entry => entry.Email));
                    Absence absence = absences.First(absence => absence.Id == entry.Id);

                    absence.AddNotification(NotificationType.Email, message.message, emails);

                    foreach (EmailRecipient recipient in recipients)
                        _logger.Information("{id}: Message sent via Email to {email} for Whole Absence on {Date}", request.JobId, recipient.Email, group.Key.ToShortDateString());
                }
            }
            else
            {
                await _emailService.SendAdminAbsenceContactAlert(student.DisplayName);
            }
        }

        return Result.Success();
    }
}

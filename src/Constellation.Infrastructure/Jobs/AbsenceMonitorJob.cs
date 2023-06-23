namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.DTOs;
using Constellation.Application.Families.GetResidentialFamilyEmailAddresses;
using Constellation.Application.Families.GetResidentialFamilyMobileNumbers;
using Constellation.Application.Features.Jobs.AbsenceMonitor.Commands;
using Constellation.Application.Features.Jobs.AbsenceMonitor.Models;
using Constellation.Application.Features.Jobs.AbsenceMonitor.Queries;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Jobs.AbsenceClassworkNotificationJob;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Families;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Constellation.Infrastructure.DependencyInjection;
using LinqKit;

public class AbsenceMonitorJob : IAbsenceMonitorJob, IHangfireJob
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ISchoolContactRepository _schoolContactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAbsenceProcessingJob _absenceProcessor;
    private readonly IEmailService _emailService;
    private readonly ISMSService _smsService;
    private readonly IAbsenceClassworkNotificationJob _classworkNotifier;
    private readonly ILogger _logger;

    public AbsenceMonitorJob(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IFamilyRepository familyRepository,
        ISchoolContactRepository schoolContactRepository,
        IUnitOfWork unitOfWork,
        IAbsenceProcessingJob absenceProcessor,
        IEmailService emailService,
        ISMSService smsService,
        IAbsenceClassworkNotificationJob classworkNotifier,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _familyRepository = familyRepository;
        _schoolContactRepository = schoolContactRepository;
        _unitOfWork = unitOfWork;
        _absenceProcessor = absenceProcessor;
        _emailService = emailService;
        _smsService = smsService;
        _classworkNotifier = classworkNotifier;
        _logger = logger.ForContext<IAbsenceMonitorJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        _logger.Information("{id}: Starting Absence Monitor Scan.", jobId);

        foreach (Grade grade in Enum.GetValues(typeof(Grade)))
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _logger.Information("{id}: Getting students from {grade}", jobId, grade);

            List<Student> students = await _studentRepository.GetCurrentStudentFromGrade(grade, cancellationToken);

            students = students
                .OrderBy(student => student.LastName)
                .ThenBy(student => student.FirstName)
                .ToList();

            _logger.Information("{id}: Found {students} students to scan.", jobId, students.Count);

            foreach (Student student in students)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                List<Absence> absences = await _absenceProcessor.StartJob(jobId, student, cancellationToken);

                if (absences.Any())
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    foreach (Absence absence in absences)
                        _absenceRepository.Insert(absence);

                    if (string.IsNullOrWhiteSpace(student.SentralStudentId))
                        continue;

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    await SendStudentNotifications(
                        jobId, 
                        student, 
                        absences.Where(absence => absence.Type == AbsenceType.Partial).ToList(), 
                        cancellationToken);

                    await SendParentNotifications(
                        jobId,
                        student,
                        absences.Where(absence => absence.Type == AbsenceType.Whole).ToList(),
                        cancellationToken);
                }

                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    await SendParentDigests(jobId, student, cancellationToken);
                    await SendCoordinatorDigests(jobId, student, cancellationToken);
                }

                await _unitOfWork.CompleteAsync(cancellationToken);
                absences = null;
            }
        }

        await _classworkNotifier.StartJob(jobId, DateOnly.FromDateTime(DateTime.Today), cancellationToken);
    }

    private async Task SendCoordinatorDigests(
        Guid jobId, 
        Student student,
        CancellationToken cancellationToken)
    {
        List<Absence> digestAbsences = await _absenceRepository.GetUnexplainedWholeAbsencesForStudentWithDelay(student.StudentId, 2, cancellationToken);

        if (digestAbsences.Any())
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

            var message = await _emailService.SendCoordinatorWholeAbsenceDigest(digestAbsences, recipients);

            if (message == null)
                return;

            string emails = string.Join(", ", recipients.Select(entry => entry.Email));

            foreach (var absence in digestAbsences)
            {
                absence.AddNotification(
                    NotificationType.Email,
                    message.message,
                    emails);

                foreach (EmailRecipient recipient in recipients)
                    _logger.Information("{id}: School digest sent to {recipient} for {student}", jobId, recipient.Name, student.DisplayName);
            }

            coordinators = null;
            recipients = null;
            digestAbsences = null;
        }
    }

    private async Task SendParentDigests(
        Guid jobId, 
        Student student, 
        CancellationToken cancellationToken = default)
    {
        var digestAbsences = await _absenceRepository.GetUnexplainedWholeAbsencesForStudentWithDelay(student.StudentId, 1, cancellationToken);

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
                EmailDtos.SentEmail sentmessage = await _emailService.SendParentWholeAbsenceDigest(digestAbsences, recipients);

                if (sentmessage == null)
                    return;

                string emails = string.Join(", ", recipients.Select(entry => entry.Email));

                foreach (Absence absence in digestAbsences)
                {
                    absence.AddNotification(
                        NotificationType.Email,
                        sentmessage.message,
                        emails);

                    foreach (var recipient in recipients)
                        _logger.Information("{id}: Parent digest sent to {address} for {student}", jobId, recipient.Email, student.DisplayName);
                }
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
    }

    private async Task SendParentNotifications(
        Guid jobId, 
        Student student, 
        List<Absence> wholeAbsences,
        CancellationToken cancellationToken = default)
    {
        // Send SMS or emails to parents
        if (wholeAbsences.Any())
        {
            List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.StudentId, cancellationToken);
            List<Parent> parents = families.SelectMany(family => family.Parents).ToList();

            List<string> numbers = parents
                .Select(parent => parent.MobileNumber)
                .Distinct()
                .ToList();

            List<PhoneNumber> phoneNumbers = new();

            foreach (var number in numbers)
            {
                Result<PhoneNumber> result = PhoneNumber.Create(number);

                if (result.IsSuccess)
                    phoneNumbers.Add(result.Value);
            }

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

            List<IGrouping<DateOnly, Absence>> groupedAbsences = wholeAbsences.GroupBy(absence => absence.Date).ToList();

            foreach (IGrouping<DateOnly, Absence> group in groupedAbsences)
            {
                if (phoneNumbers.Any() && group.Key == DateOnly.FromDateTime(DateTime.Today.AddDays(-1)))
                {
                    var sentMessages = await _smsService.SendAbsenceNotificationAsync(group.ToList(), phoneNumbers);

                    if (sentMessages == null)
                    {
                        // SMS Gateway failed. Send via email instead.
                        _logger.Warning("{id}: SMS Sending Failed! Fallback to Email notifications.", jobId);

                        if (recipients.Any())
                        {
                            EmailDtos.SentEmail message = await _emailService.SendParentWholeAbsenceAlert(group.ToList(), recipients);

                            foreach (var absence in group)
                            {
                                string emails = string.Join(", ", recipients.Select(entry => entry.Email));

                                absence.AddNotification(NotificationType.Email, message.message, emails);

                                foreach (var recipient in recipients)
                                    _logger.Information("{id}: Message sent via Email to {email} for Whole Absence on {Date}", jobId, recipient.Email, group.Key.ToShortDateString());
                            }
                        }
                        else
                        {
                            _logger.Error("{id}: Email addresses not found! Parents have not been notified!", jobId);
                        }
                    }

                    // Once the message has been sent, add it to the database.
                    if (sentMessages.Messages.Count > 0)
                    {
                        foreach (var absence in group)
                        {
                            string sentToNumbers = string.Join(", ", phoneNumbers.Select(entry => entry.ToString(PhoneNumber.Format.Mobile)));

                            absence.AddNotification(NotificationType.SMS, sentMessages.Messages.First().MessageBody, sentToNumbers);

                            foreach (var number in phoneNumbers)
                                _logger.Information("{id}: Message sent via SMS to {number} for Whole Absence on {Date}", jobId, number.ToString(PhoneNumber.Format.Mobile), group.Key.ToShortDateString());
                        }
                    }
                }
                else if (recipients.Any())
                {
                    EmailDtos.SentEmail message = await _emailService.SendParentWholeAbsenceAlert(group.ToList(), recipients);

                    foreach (var absence in group)
                    {
                        string emails = string.Join(", ", recipients.Select(entry => entry.Email));

                        absence.AddNotification(NotificationType.Email, message.message, emails);

                        foreach (var recipient in recipients)
                            _logger.Information("{id}: Message sent via Email to {email} for Whole Absence on {Date}", jobId, recipient.Email, group.Key.ToShortDateString());
                    }
                }
                else
                {
                    await _emailService.SendAdminAbsenceContactAlert(student.DisplayName).ConfigureAwait(false);
                }
            }

            groupedAbsences = null;
        }

        wholeAbsences = null;
    }

    private async Task SendStudentNotifications(
        Guid jobId, 
        Student student, 
        List<Absence> partialAbsences, 
        CancellationToken cancellationToken = default)
    {
        // Send emails to students
        if (partialAbsences.Any())
        {
            var recipients = new List<string> { student.EmailAddress };

            var sentEmail = await _emailService.SendStudentPartialAbsenceExplanationRequest(partialAbsences, recipients);

            foreach (var absence in partialAbsences)
            {
                absence.AddNotification(NotificationType.Email, sentEmail.message, student.EmailAddress);

                foreach (var email in recipients)
                    _logger.Information("{id}: Message sent via Email to {email} for Partial Absence on {Date}", jobId, email, absence.Date.ToShortDateString());
            }

            partialAbsences = null;
        }
    }
}

namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Families.GetResidentialFamilyEmailAddresses;
using Constellation.Application.Families.GetResidentialFamilyMobileNumbers;
using Constellation.Application.Features.Jobs.AbsenceMonitor.Commands;
using Constellation.Application.Features.Jobs.AbsenceMonitor.Models;
using Constellation.Application.Features.Jobs.AbsenceMonitor.Queries;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Jobs.AbsenceClassworkNotificationJob;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Infrastructure.DependencyInjection;

public class AbsenceMonitorJob : IAbsenceMonitorJob, IHangfireJob
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAbsenceProcessingJob _absenceProcessor;
    private readonly IAbsenceClassworkNotificationJob _classworkNotifier;
    private readonly ILogger _logger;

    public AbsenceMonitorJob(
        IStudentRepository studentRepository,

        IUnitOfWork unitOfWork,
        IAbsenceProcessingJob absenceProcessor,
        IAbsenceClassworkNotificationJob classworkNotifier,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _absenceProcessor = absenceProcessor;
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

                    await _mediator.Send(new AddNewStudentAbsencesCommand { Absences = absences }, cancellationToken).ConfigureAwait(false);

                    if (string.IsNullOrWhiteSpace(student.SentralStudentId))
                        continue;

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    await SendStudentNotifications(jobId, student);
                    await SendParentNotifications(jobId, student, cancellationToken);
                }

                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    await SendParentDigests(jobId, student, cancellationToken);
                    await SendCoordinatorDigests(jobId, student);
                }

                absences?.Clear();
                //_unitOfWork.ClearTrackerDb();
            }
        }

        await _classworkNotifier.StartJob(jobId, DateOnly.FromDateTime(DateTime.Today), cancellationToken);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }

    private async Task SendCoordinatorDigests(Guid jobId, StudentForAbsenceScan student)
    {
        var coordinatorDigestAbsences = await _mediator.Send(new GetUnexplainedAbsencesForDigestQuery { StudentId = student.StudentId, Type = Absence.Whole, AgeInWeeks = 2 }).ConfigureAwait(false);

        if (coordinatorDigestAbsences.Any())
        {
            var message = await _emailService.SendCoordinatorWholeAbsenceDigest(coordinatorDigestAbsences.ToList()).ConfigureAwait(false);

            if (message == null)
                return;

            foreach (var absence in coordinatorDigestAbsences)
                await _mediator.Send(new AddNewAbsenceNotificationCommand
                {
                    AbsenceId = absence.Id,
                    Type = NotificationType.Email,
                    MessageBody = message.message,
                    Recipients = new List<string> { "School Contacts" },
                    MessageId = message.id
                }).ConfigureAwait(false);

            _logger.Information("{id}: School digest sent to {School} for {student}", jobId, student.SchoolName, student.DisplayName);

            coordinatorDigestAbsences?.Clear();
        }
    }

    private async Task SendParentDigests(Guid jobId, StudentForAbsenceScan student, CancellationToken cancellationToken = default)
    {
        var parentDigestAbsences = await _mediator.Send(new GetUnexplainedAbsencesForDigestQuery { StudentId = student.StudentId, Type = Absence.Whole, AgeInWeeks = 1 }, cancellationToken).ConfigureAwait(false);

        if (parentDigestAbsences.Any())
        {
            var emailAddresses = await _mediator.Send(new GetResidentialFamilyEmailAddressesQuery(student.StudentId), cancellationToken).ConfigureAwait(false);

            if (emailAddresses.IsSuccess)
            {
                var sentmessage = await _emailService.SendParentWholeAbsenceDigest(parentDigestAbsences.ToList(), emailAddresses.Value).ConfigureAwait(false);

                if (sentmessage == null)
                    return;

                foreach (var absence in parentDigestAbsences)
                    await _mediator.Send(new AddNewAbsenceNotificationCommand
                    {
                        AbsenceId = absence.Id,
                        Type = NotificationType.Email,
                        MessageBody = sentmessage.message,
                        Recipients = new List<string> { "School Contacts" },
                        MessageId = sentmessage.id
                    }, cancellationToken).ConfigureAwait(false);

                foreach (var address in emailAddresses.Value)
                {
                    _logger.Information("{id}: Parent digest sent to {address} for {student}", jobId, address.Email, student.DisplayName);
                }
            }
            else
            {
                await _emailService.SendAdminAbsenceContactAlert(student.DisplayName).ConfigureAwait(false);
            }

            parentDigestAbsences?.Clear();
        }
    }

    private async Task SendParentNotifications(Guid jobId, StudentForAbsenceScan student, CancellationToken cancellationToken = default)
    {
        var phoneNumbers = await _mediator.Send(new GetResidentialFamilyMobileNumbersQuery(student.StudentId), cancellationToken).ConfigureAwait(false);
        var emailAddresses = await _mediator.Send(new GetResidentialFamilyEmailAddressesQuery(student.StudentId), cancellationToken).ConfigureAwait(false);

        var recentWholeAbsences = await _mediator.Send(new GetRecentAbsencesForStudentQuery { StudentId = student.StudentId, AbsenceType = Absence.Whole }, cancellationToken).ConfigureAwait(false);

        // Send SMS or emails to parents
        if (recentWholeAbsences.Any())
        {
            var groupedAbsences = recentWholeAbsences.GroupBy(absence => absence.Date).ToList();

            foreach (var group in groupedAbsences)
            {
                if (phoneNumbers.IsSuccess && phoneNumbers.Value.Any() && group.Key.Date == DateTime.Today.AddDays(-1).Date)
                {
                    var sentMessages = await _smsService.SendAbsenceNotificationAsync(group.ToList(), phoneNumbers.Value).ConfigureAwait(false);

                    if (sentMessages == null)
                    {
                        // SMS Gateway failed. Send via email instead.
                        _logger.Warning("{id}: SMS Sending Failed! Fallback to Email notifications.", jobId);

                        if (emailAddresses.IsSuccess && emailAddresses.Value.Any())
                        {
                            var message = await _emailService.SendParentWholeAbsenceAlert(group.ToList(), emailAddresses.Value).ConfigureAwait(false);

                            foreach (var absence in group)
                                await _mediator.Send(new AddNewAbsenceNotificationCommand
                                {
                                    AbsenceId = absence.Id,
                                    Type = NotificationType.Email,
                                    MessageBody = message.message,
                                    MessageId = message.id,
                                    Recipients = emailAddresses.Value.Select(entry => entry.Email).ToList()
                                }, cancellationToken).ConfigureAwait(false);

                            foreach (var email in emailAddresses.Value)
                                _logger.Information("{id}: Message sent via Email to {email} for Whole Absence on {Date}", jobId, email.Email, group.Key.Date.ToShortDateString());
                        }
                        else
                        {
                            _logger.LogError("{id}: Email addresses not found! Parents have not been notified!", jobId);
                        }
                    }

                    // Once the message has been sent, add it to the database.
                    if (sentMessages.Messages.Count > 0)
                    {
                        foreach (var absence in group)
                            await _mediator.Send(new AddNewAbsenceNotificationCommand
                            {
                                AbsenceId = absence.Id,
                                Type = NotificationType.SMS,
                                MessageBody = sentMessages.Messages.First().MessageBody,
                                MessageId = sentMessages.Messages.First().OutgoingId,
                                Recipients = phoneNumbers.Value.Select(entry => entry.ToString(Core.ValueObjects.PhoneNumber.Format.None)).ToList()
                            }, cancellationToken).ConfigureAwait(false);

                        foreach (var number in phoneNumbers.Value)
                            _logger.Information("{id}: Message sent via SMS to {number} for Whole Absence on {Date}", jobId, number.ToString(Core.ValueObjects.PhoneNumber.Format.Mobile), group.Key.Date.ToShortDateString());
                    }
                }
                else if (emailAddresses.IsSuccess && emailAddresses.Value.Any())
                {
                    var message = await _emailService.SendParentWholeAbsenceAlert(group.ToList(), emailAddresses.Value).ConfigureAwait(false);

                    foreach (var absence in group)
                        await _mediator.Send(new AddNewAbsenceNotificationCommand
                        {
                            AbsenceId = absence.Id,
                            Type = NotificationType.Email,
                            MessageBody = message.message,
                            MessageId = message.id,
                            Recipients = emailAddresses.Value.Select(entry => entry.Email).ToList()
                        }, cancellationToken).ConfigureAwait(false);

                    foreach (var email in emailAddresses.Value)
                        _logger.Information("{id}: Message sent via Email to {email} for Whole Absence on {Date}", jobId, email.Email, group.Key.Date.ToShortDateString());
                }
                else
                {
                    await _emailService.SendAdminAbsenceContactAlert(student.DisplayName).ConfigureAwait(false);
                }
            }

            groupedAbsences?.Clear();
        }

        recentWholeAbsences?.Clear();
    }

    private async Task SendStudentNotifications(Guid jobId, StudentForAbsenceScan student)
    {
        var recentPartialAbsences = await _mediator.Send(new GetRecentAbsencesForStudentQuery { StudentId = student.StudentId, AbsenceType = AbsenceType.Partial }).ConfigureAwait(false);

        // Send emails to students
        if (recentPartialAbsences.Any())
        {
            var recipients = new List<string> { student.EmailAddress };

            var sentEmail = await _emailService.SendStudentPartialAbsenceExplanationRequest(recentPartialAbsences.ToList(), recipients).ConfigureAwait(false);

            foreach (var absence in recentPartialAbsences)
            {
                await _mediator.Send(new AddNewAbsenceNotificationCommand
                {
                    AbsenceId = absence.Id,
                    Type = NotificationType.Email,
                    MessageBody = sentEmail.message,
                    MessageId = sentEmail.id,
                    Recipients = recipients
                }).ConfigureAwait(false);
            }

            foreach (var email in recipients)
                _logger.Information("{id}: Message sent via Email to {email} for Partial Absence on {Date}", jobId, email, recentPartialAbsences.First().Date.ToShortDateString());

            recentPartialAbsences?.Clear();
        }
    }
}

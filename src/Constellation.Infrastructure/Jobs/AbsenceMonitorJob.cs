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
using Constellation.Core.Models.Absences;
using Constellation.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Constellation.Infrastructure.Jobs
{
    public class AbsenceMonitorJob : IAbsenceMonitorJob, IScopedService, IHangfireJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAbsenceProcessingJob _absenceProcessor;
        private readonly IEmailService _emailService;
        private readonly ISMSService _smsService;
        private readonly IAbsenceClassworkNotificationJob _classworkNotifier;
        private readonly ILogger<IAbsenceMonitorJob> _logger;
        private readonly IMediator _mediator;

        public AbsenceMonitorJob(
            IUnitOfWork unitOfWork,
            IAbsenceProcessingJob absenceProcessor,
            IEmailService emailService,
            ISMSService smsService,
            IAbsenceClassworkNotificationJob classworkNotifier,
            ILogger<IAbsenceMonitorJob> logger,
            IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _absenceProcessor = absenceProcessor;
            _emailService = emailService;
            _smsService = smsService;
            _classworkNotifier = classworkNotifier;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task StartJob(Guid jobId, CancellationToken token)
        {
            _logger.LogInformation("{id}: Starting Absence Monitor Scan.", jobId);

            foreach (Grade grade in Enum.GetValues(typeof(Grade)))
            {
                if (token.IsCancellationRequested)
                    return;

                _logger.LogInformation("{id}: Getting students from {grade}", jobId, grade);

                var students = await _mediator.Send(new GetStudentsForAbsenceScanQuery { Grade = grade }, token);
                
                students = students.OrderBy(student => student.CurrentGrade).ThenBy(student => student.LastName).ThenBy(student => student.FirstName).ToList();

                _logger.LogInformation("{id}: Found {students} students to scan.", jobId, students.Count);

                foreach (var student in students)
                {
                    if (token.IsCancellationRequested)
                        return;

                    var absences = await _absenceProcessor.StartJob(jobId, student, token);

                    if (absences.Any())
                    {
                        if (token.IsCancellationRequested)
                            return;

                        await _mediator.Send(new AddNewStudentAbsencesCommand { Absences = absences }, token);

                        if (string.IsNullOrWhiteSpace(student.SentralStudentId))
                            continue;

                        if (token.IsCancellationRequested)
                            return;

                        await SendStudentNotifications(jobId, student);
                        await SendParentNotifications(jobId, student, token);
                    }

                    if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        await SendParentDigests(jobId, student, token);
                        await SendCoordinatorDigests(jobId, student);
                    }

                    absences?.Clear();
                    //_unitOfWork.ClearTrackerDb();
                }
            }

            await _classworkNotifier.StartJob(jobId, DateTime.Today, token);

            await _unitOfWork.CompleteAsync(token);
        }

        private async Task SendCoordinatorDigests(Guid jobId, StudentForAbsenceScan student)
        {
            var coordinatorDigestAbsences = await _mediator.Send(new GetUnexplainedAbsencesForDigestQuery { StudentId = student.StudentId, Type = Absence.Whole, AgeInWeeks = 2 });

            if (coordinatorDigestAbsences.Any())
            {
                var message = await _emailService.SendCoordinatorWholeAbsenceDigest(coordinatorDigestAbsences.ToList());

                if (message == null)
                    return;

                foreach (var absence in coordinatorDigestAbsences)
                    await _mediator.Send(new AddNewAbsenceNotificationCommand
                    {
                        AbsenceId = absence.Id,
                        Type = AbsenceNotification.Email,
                        MessageBody = message.message,
                        Recipients = new List<string> { "School Contacts" },
                        MessageId = message.id
                    });

                _logger.LogInformation("{id}: School digest sent to {School} for {student}", jobId, student.SchoolName, student.DisplayName);

                coordinatorDigestAbsences?.Clear();
            }
        }

        private async Task SendParentDigests(Guid jobId, StudentForAbsenceScan student, CancellationToken cancellationToken = default)
        {
            var parentDigestAbsences = await _mediator.Send(new GetUnexplainedAbsencesForDigestQuery { StudentId = student.StudentId, Type = Absence.Whole, AgeInWeeks = 1 }, cancellationToken);

            if (parentDigestAbsences.Any())
            {
                var emailAddresses = await _mediator.Send(new GetResidentialFamilyEmailAddressesQuery(student.StudentId), cancellationToken);

                if (emailAddresses.IsSuccess)
                {
                    var sentmessage = await _emailService.SendParentWholeAbsenceDigest(parentDigestAbsences.ToList(), emailAddresses.Value);

                    if (sentmessage == null)
                        return;

                    foreach (var absence in parentDigestAbsences)
                        await _mediator.Send(new AddNewAbsenceNotificationCommand
                        {
                            AbsenceId = absence.Id,
                            Type = AbsenceNotification.Email,
                            MessageBody = sentmessage.message,
                            Recipients = new List<string> { "School Contacts" },
                            MessageId = sentmessage.id
                        }, cancellationToken);

                    foreach (var address in emailAddresses.Value)
                    {
                        _logger.LogInformation("{id}: Parent digest sent to {address} for {student}", jobId, address.Email, student.DisplayName);
                    }
                }
                else
                {
                    await _emailService.SendAdminAbsenceContactAlert(student.DisplayName);
                }

                parentDigestAbsences?.Clear();
            }
        }

        private async Task SendParentNotifications(Guid jobId, StudentForAbsenceScan student, CancellationToken cancellationToken = default)
        {
            var phoneNumbers = await _mediator.Send(new GetResidentialFamilyMobileNumbersQuery(student.StudentId), cancellationToken);
            var emailAddresses = await _mediator.Send(new GetResidentialFamilyEmailAddressesQuery(student.StudentId), cancellationToken);

            var recentWholeAbsences = await _mediator.Send(new GetRecentAbsencesForStudentQuery { StudentId = student.StudentId, AbsenceType = Absence.Whole }, cancellationToken);

            // Send SMS or emails to parents
            if (recentWholeAbsences.Any())
            {
                var groupedAbsences = recentWholeAbsences.GroupBy(absence => absence.Date).ToList();

                foreach (var group in groupedAbsences)
                {
                    if (phoneNumbers.IsSuccess && phoneNumbers.Value.Any() && group.Key.Date == DateTime.Today.AddDays(-1).Date)
                    {
                        var sentMessages = await _smsService.SendAbsenceNotificationAsync(group.ToList(), phoneNumbers.Value);

                        if (sentMessages == null)
                        {
                            // SMS Gateway failed. Send via email instead.
                            _logger.LogWarning("{id}: SMS Sending Failed! Fallback to Email notifications.", jobId);

                            if (emailAddresses.IsSuccess && emailAddresses.Value.Any())
                            {
                                var message = await _emailService.SendParentWholeAbsenceAlert(group.ToList(), emailAddresses.Value);

                                foreach (var absence in group)
                                    await _mediator.Send(new AddNewAbsenceNotificationCommand
                                    {
                                        AbsenceId = absence.Id,
                                        Type = AbsenceNotification.Email,
                                        MessageBody = message.message,
                                        MessageId = message.id,
                                        Recipients = emailAddresses.Value.Select(entry => entry.Email).ToList()
                                    }, cancellationToken);

                                foreach (var email in emailAddresses.Value)
                                    _logger.LogInformation("{id}: Message sent via Email to {email} for Whole Absence on {Date}", jobId, email.Email, group.Key.Date.ToShortDateString());
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
                                    Type = AbsenceNotification.SMS,
                                    MessageBody = sentMessages.Messages.First().MessageBody,
                                    MessageId = sentMessages.Messages.First().OutgoingId,
                                    Recipients = phoneNumbers.Value.Select(entry => entry.ToString(Core.ValueObjects.PhoneNumber.Format.None)).ToList()
                                }, cancellationToken);

                            foreach (var number in phoneNumbers.Value)
                                _logger.LogInformation("{id}: Message sent via SMS to {number} for Whole Absence on {Date}", jobId, number.ToString(Core.ValueObjects.PhoneNumber.Format.Mobile), group.Key.Date.ToShortDateString());
                        }
                    }
                    else if (emailAddresses.IsSuccess && emailAddresses.Value.Any())
                    {
                        var message = await _emailService.SendParentWholeAbsenceAlert(group.ToList(), emailAddresses.Value);

                        foreach (var absence in group)
                            await _mediator.Send(new AddNewAbsenceNotificationCommand
                            {
                                AbsenceId = absence.Id,
                                Type = AbsenceNotification.Email,
                                MessageBody = message.message,
                                MessageId = message.id,
                                Recipients = emailAddresses.Value.Select(entry => entry.Email).ToList()
                            }, cancellationToken);

                        foreach (var email in emailAddresses.Value)
                            _logger.LogInformation("{id}: Message sent via Email to {email} for Whole Absence on {Date}", jobId, email.Email, group.Key.Date.ToShortDateString());
                    }
                    else
                    {
                        await _emailService.SendAdminAbsenceContactAlert(student.DisplayName);
                    }
                }

                groupedAbsences?.Clear();
            }

            recentWholeAbsences?.Clear();
        }

        private async Task SendStudentNotifications(Guid jobId, StudentForAbsenceScan student)
        {
            var recentPartialAbsences = await _mediator.Send(new GetRecentAbsencesForStudentQuery { StudentId = student.StudentId, AbsenceType = Absence.Partial });

            // Send emails to students
            if (recentPartialAbsences.Any())
            {
                var recipients = new List<string> { student.EmailAddress };

                var sentEmail = await _emailService.SendStudentPartialAbsenceExplanationRequest(recentPartialAbsences.ToList(), recipients);

                foreach (var absence in recentPartialAbsences)
                {
                    await _mediator.Send(new AddNewAbsenceNotificationCommand
                    {
                        AbsenceId = absence.Id,
                        Type = AbsenceNotification.Email,
                        MessageBody = sentEmail.message,
                        MessageId = sentEmail.id,
                        Recipients = recipients
                    });
                }

                foreach (var email in recipients)
                    _logger.LogInformation("{id}: Message sent via Email to {email} for Partial Absence on {Date}", jobId, email, recentPartialAbsences.First().Date.ToShortDateString());

                recentPartialAbsences?.Clear();
            }
        }
    }
}

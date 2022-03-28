using Constellation.Application.Common.CQRS.Jobs.AbsenceMonitor.Command;
using Constellation.Application.Common.CQRS.Jobs.AbsenceMonitor.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class AbsenceMonitorJob : IAbsenceMonitorJob, IScopedService, IHangfireJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAbsenceProcessingJob _absenceProcessor;
        private readonly ISentralGateway _sentralService;
        private readonly IEmailService _emailService;
        private readonly ISMSService _smsService;
        private readonly IAbsenceClassworkNotificationJob _classworkNotifier;
        private readonly ILogger<IAbsenceMonitorJob> _logger;
        private readonly IMediator _mediator;

        public AbsenceMonitorJob(IUnitOfWork unitOfWork, IAbsenceProcessingJob absenceProcessor, 
            ISentralGateway sentralService, IEmailService emailService, 
            ISMSService smsService, IAbsenceClassworkNotificationJob classworkNotifier,
            ILogger<IAbsenceMonitorJob> logger, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _absenceProcessor = absenceProcessor;
            _sentralService = sentralService;
            _emailService = emailService;
            _smsService = smsService;
            _classworkNotifier = classworkNotifier;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task StartJob(bool automated)
        {
            if (automated)
            {
                var jobStatus = await _unitOfWork.JobActivations.GetForJob(nameof(IAbsenceMonitorJob));
                if (jobStatus == null || !jobStatus.IsActive)
                {
                    _logger.LogInformation("Stopped due to job being set inactive.");
                    return;
                }
            }

            _logger.LogInformation("Starting Absence Monitor Scan.");

            foreach (Grade grade in Enum.GetValues(typeof(Grade)))
            {
                _logger.LogInformation("Getting students from {grade}", grade);

                var students = await _mediator.Send(new GetStudentsForAbsenceScanQuery { Grade = grade });
                
                students = students.OrderBy(student => student.CurrentGrade).ThenBy(student => student.LastName).ThenBy(student => student.FirstName).ToList();

                _logger.LogInformation("Found {students} students to scan.", students.Count);

                foreach (var student in students)
                {
                    var absences = await _absenceProcessor.StartJob(student);

                    if (absences.Any())
                    {
                        await _mediator.Send(new AddNewStudentAbsencesCommand { Absences = absences });

                        if (string.IsNullOrWhiteSpace(student.SentralStudentId))
                            continue;

                        await SendStudentNotifications(student);
                        await SendParentNotifications(student);
                    }

                    if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                    {
                        await SendParentDigests(student);
                        await SendCoordinatorDigests(student);
                    }

                    absences?.Clear();
                    //_unitOfWork.ClearTrackerDb();
                }
            }

            await _classworkNotifier.StartJob(DateTime.Today);

            await _unitOfWork.CompleteAsync();
        }

        private async Task SendCoordinatorDigests(StudentForAbsenceScan student)
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

                _logger.LogInformation("  School digest sent to {School} for {student}", student.SchoolName, student.DisplayName);

                coordinatorDigestAbsences?.Clear();
            }
        }

        private async Task SendParentDigests(StudentForAbsenceScan student)
        {
            var parentDigestAbsences = await _mediator.Send(new GetUnexplainedAbsencesForDigestQuery { StudentId = student.StudentId, Type = Absence.Whole, AgeInWeeks = 1 });

            if (parentDigestAbsences.Any())
            {
                var emailAddresses = await _sentralService.GetContactEmailsAsync(student.SentralStudentId);

                if (emailAddresses.Any())
                {
                    var sentmessage = await _emailService.SendParentWholeAbsenceDigest(parentDigestAbsences.ToList(), emailAddresses);

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
                        });

                    foreach (var address in emailAddresses)
                    {
                        _logger.LogInformation("  Parent digest sent to {address} for {student}", address, student.DisplayName);
                    }
                }
                else
                {
                    await _emailService.SendAdminAbsenceContactAlert(student.DisplayName);
                }

                parentDigestAbsences?.Clear();
            }
        }

        private async Task SendParentNotifications(StudentForAbsenceScan student)
        {
            var phoneNumbers = await _sentralService.GetContactNumbersAsync(student.SentralStudentId);
            var emailAddresses = await _sentralService.GetContactEmailsAsync(student.SentralStudentId);

            var recentWholeAbsences = await _mediator.Send(new GetRecentAbsencesForStudentQuery { StudentId = student.StudentId, AbsenceType = Absence.Whole });

            // Send SMS or emails to parents
            if (recentWholeAbsences.Any())
            {
                var groupedAbsences = recentWholeAbsences.GroupBy(absence => absence.Date).ToList();

                foreach (var group in groupedAbsences)
                {
                    if (phoneNumbers.Any() && group.Key.Date == DateTime.Today.AddDays(-1).Date)
                    {
                        var sentMessages = await _smsService.SendAbsenceNotificationAsync(group.ToList(), phoneNumbers);

                        if (sentMessages == null)
                        {
                            // SMS Gateway failed. Send via email instead.
                            _logger.LogWarning("  SMS Sending Failed! Fallback to Email notifications.");

                            if (emailAddresses.Any())
                            {
                                var message = await _emailService.SendParentWholeAbsenceAlert(group.ToList(), emailAddresses);

                                foreach (var absence in group)
                                    await _mediator.Send(new AddNewAbsenceNotificationCommand 
                                    { 
                                        AbsenceId = absence.Id, 
                                        Type = AbsenceNotification.Email, 
                                        MessageBody = message.message, 
                                        MessageId = message.id, 
                                        Recipients = emailAddresses 
                                    });

                                foreach (var email in emailAddresses)
                                    _logger.LogInformation("  Message sent via Email to {email} for Whole Absence on {Date}", email, group.Key.Date.ToShortDateString());
                            }
                            else
                            {
                                _logger.LogError("  Email addresses not found! Parents have not been notified!");
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
                                    Recipients = phoneNumbers
                                });

                            foreach (var number in phoneNumbers)
                                _logger.LogInformation("  Message sent via SMS to {number} for Whole Absence on {Date}", number, group.Key.Date.ToShortDateString());
                        }
                    }
                    else if (emailAddresses.Any())
                    {
                        var message = await _emailService.SendParentWholeAbsenceAlert(group.ToList(), emailAddresses);

                        foreach (var absence in group)
                            await _mediator.Send(new AddNewAbsenceNotificationCommand
                            {
                                AbsenceId = absence.Id,
                                Type = AbsenceNotification.Email,
                                MessageBody = message.message,
                                MessageId = message.id,
                                Recipients = emailAddresses
                            });

                        foreach (var email in emailAddresses)
                            _logger.LogInformation("  Message sent via Email to {email} for Whole Absence on {Date}", email, group.Key.Date.ToShortDateString());
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

        private async Task SendStudentNotifications(StudentForAbsenceScan student)
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
                    _logger.LogInformation("  Message sent via Email to {email} for Partial Absence on {Date}", email, recentPartialAbsences.First().Date.ToShortDateString());

                recentPartialAbsences?.Clear();
            }
        }
    }
}

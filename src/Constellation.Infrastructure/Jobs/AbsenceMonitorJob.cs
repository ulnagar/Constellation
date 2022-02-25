using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
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

        public AbsenceMonitorJob(IUnitOfWork unitOfWork, IAbsenceProcessingJob absenceProcessor, 
            ISentralGateway sentralService, IEmailService emailService, 
            ISMSService smsService, IAbsenceClassworkNotificationJob classworkNotifier,
            ILogger<IAbsenceMonitorJob> logger)
        {
            _unitOfWork = unitOfWork;
            _absenceProcessor = absenceProcessor;
            _sentralService = sentralService;
            _emailService = emailService;
            _smsService = smsService;
            _classworkNotifier = classworkNotifier;
            _logger = logger;
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

            var students = new List<Student>();

            foreach (Grade grade in Enum.GetValues(typeof(Grade)))
            {
                var gradeStudents = await _unitOfWork.Students.ForAbsenceScan(grade);
                
                students.AddRange(gradeStudents);
            }

            //var students = await _unitOfWork.Students.ForAbsenceScan();
            _logger.LogInformation($"Found {students.Count} students to scan.");

            students = students.OrderBy(student => student.CurrentGrade).ThenBy(student => student.LastName).ThenBy(student => student.FirstName).ToList();

            foreach (var student in students)
            {
                var absences = await _absenceProcessor.StartJob(student);

                if (absences.Any())
                {
                    student.Absences.AddRange(absences);
                    await _unitOfWork.CompleteAsync();

                    if (string.IsNullOrWhiteSpace(student.SentralStudentId))
                    continue;

                    var phoneNumbers = await _sentralService.GetContactNumbersAsync(student.SentralStudentId);
                    var emailAddresses = await _sentralService.GetContactEmailsAsync(student.SentralStudentId);

                    var recentPartialAbsences = absences.Where(absence =>
                            absence.Explained == false &&
                            absence.Type == Absence.Partial &&
                            absence.DateScanned.Date == DateTime.Today)
                        .ToList();

                    // Send emails to students
                    if (recentPartialAbsences.Any())
                    {
                        var recipients = new List<string> { student.EmailAddress };

                        var sentEmail = await _emailService.SendStudentPartialAbsenceExplanationRequest(recentPartialAbsences, recipients);

                        foreach (var absence in recentPartialAbsences)
                        {
                            absence.Notifications.Add(new AbsenceNotification
                            {
                                Type = AbsenceNotification.Email,
                                Message = sentEmail.message,
                                SentAt = DateTime.Now,
                                Recipients = sentEmail.recipients,
                                OutgoingId = sentEmail.id
                            });
                        }

                        foreach (var email in recipients)
                            _logger.LogInformation($"  Message sent via Email to {email} for Partial Absence on {absences.First().Date.ToShortDateString()}");
                    }

                    var recentWholeAbsences = absences.Where(absence =>
                            absence.Explained == false &&
                            absence.Type == Absence.Whole &&
                            absence.DateScanned.Date == DateTime.Today)
                        .ToList();

                    // Send SMS or emails to parents
                    if (recentWholeAbsences.Any())
                    {
                        var groupedAbsences = recentWholeAbsences.GroupBy(absence => absence.Date).ToList();

                        foreach (var group in groupedAbsences)
                        {
                            if (phoneNumbers.Any() && group.Key.Date == DateTime.Today.AddDays(-1).Date)
                            {
                                var sentMessages = await _smsService.SendAbsenceNotificationAsync(group.ToList(), phoneNumbers); ;

                                // Once the message has been sent, add it to the database.
                                if (sentMessages.Messages.Count > 0)
                                {
                                    foreach (var absence in group)
                                    {
                                        absence.Notifications.Add(new AbsenceNotification
                                        {
                                            Type = AbsenceNotification.SMS,
                                            SentAt = DateTime.Now,
                                            Message = sentMessages.Messages.First().MessageBody,
                                            Recipients = string.Join(", ", phoneNumbers),
                                            OutgoingId = sentMessages.Messages.First().OutgoingId
                                        });
                                    }

                                    foreach (var number in phoneNumbers)
                                        _logger.LogInformation($"  Message sent via SMS to {number} for Whole Absence on {absences.First().Date.ToShortDateString()}");
                                }
                            }
                            else if (emailAddresses.Any())
                            {
                                var message = await _emailService.SendParentWholeAbsenceAlert(group.ToList(), emailAddresses);

                                foreach (var absence in group)
                                {
                                    absence.Notifications.Add(new AbsenceNotification
                                    {
                                        Type = AbsenceNotification.Email,
                                        Message = message.message,
                                        SentAt = DateTime.Now,
                                        Recipients = string.Join(", ", emailAddresses),
                                        OutgoingId = message.id
                                    });
                                }

                                foreach (var email in emailAddresses)
                                    _logger.LogInformation($"  Message sent via Email to {email} for Whole Absence on {absences.First().Date.ToShortDateString()}");
                            }
                            else
                            {
                                await _emailService.SendAdminAbsenceContactAlert(student);
                            }
                        }
                    }

                }

                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                {
                    var parentDigestAbsences = student.Absences
                        .Where(absence =>
                            !absence.Explained &&
                            absence.Type == Absence.Whole &&
                            absence.DateScanned >= DateTime.Today.AddDays(-7) &&
                            absence.DateScanned <= DateTime.Today.AddDays(-1))
                        .ToList();

                    var emailAddresses = await _sentralService.GetContactEmailsAsync(student.SentralStudentId);

                    if (parentDigestAbsences.Any())
                    {
                        if (emailAddresses.Any())
                        {
                            var sentmessage = await _emailService.SendParentWholeAbsenceDigest(parentDigestAbsences, emailAddresses);

                            foreach (var absence in parentDigestAbsences)
                            {
                                absence.Notifications.Add(new AbsenceNotification
                                {
                                    Type = AbsenceNotification.Email,
                                    Message = sentmessage.message,
                                    SentAt = DateTime.Now,
                                    Recipients = string.Join(", ", emailAddresses),
                                    OutgoingId = sentmessage.id
                                });
                            }
                        }
                        else
                        {
                            await _emailService.SendAdminAbsenceContactAlert(student);
                        }
                    }

                    var coordinatorDigestAbsences = student.Absences
                        .Where(absence =>
                            !absence.Explained &&
                            absence.Type == Absence.Whole &&
                            absence.DateScanned >= DateTime.Today.AddDays(-14) &&
                            absence.DateScanned <= DateTime.Today.AddDays(-8))
                        .ToList();

                    if (coordinatorDigestAbsences.Any())
                    {
                        var message = await _emailService.SendCoordinatorWholeAbsenceDigest(coordinatorDigestAbsences);

                        foreach (var absence in coordinatorDigestAbsences)
                        {
                            absence.Notifications.Add(new AbsenceNotification
                            {
                                Type = AbsenceNotification.Email,
                                Message = message.message,
                                SentAt = DateTime.Now,
                                Recipients = string.Join(", ", emailAddresses),
                                OutgoingId = message.id
                            });
                        }
                    }
                }
            }

            await _classworkNotifier.StartJob(DateTime.Today);

            await _unitOfWork.CompleteAsync();
        }
    }
}

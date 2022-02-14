using Constellation.Application.DTOs;
using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs

{
    public class LessonNotificationsJob : ILessonNotificationsJob, IScopedService, IHangfireJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogHandler<ILessonNotificationsJob> _logger;

        public LessonNotificationsJob(IUnitOfWork unitOfWork, IEmailService emailService,
            ILogHandler<ILessonNotificationsJob> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task StartJob()
        {
            _logger.Log(LogSeverity.Information, $"Service started.");

            var settings = await _unitOfWork.Settings.Get();
            var coordinatorEmail = settings.LessonsCoordinatorEmail;
            var headTeacherEmail = settings.LessonsHeadTeacherEmail;
            var facultyContacts = new List<EmailBaseClass.Recipient> { new EmailBaseClass.Recipient { Name = coordinatorEmail, Email = coordinatorEmail }, new EmailBaseClass.Recipient { Name = headTeacherEmail, Email = headTeacherEmail } };

            var schools = new List<EmailDtos.LessonEmail>();

            var allLessons = await _unitOfWork.Lessons.GetAllForNotifications();

            var lessons = allLessons
                .Where(lesson => lesson.DueDate < DateTime.Today)
                .ToList();

            foreach (var lesson in lessons)
            {
                foreach (var roll in lesson.Rolls.Where(roll => roll.Status == LessonStatus.Active))
                {
                    var course = lesson.Offerings.First().Course;

                    var description = $"{course.Grade} {lesson.Name}";

                    if (course.Grade == Grade.Y11 || course.Grade == Grade.Y12)
                    {
                        description = $"{course.Name} {lesson.Name}";
                    }

                    var overdue = (DateTime.Today.Subtract(lesson.DueDate).Days / 7) + 1;

                    var schoolEmail = schools.FirstOrDefault(email => email.SchoolCode == roll.SchoolCode);

                    if (schoolEmail == null)
                    {
                        var email = new EmailDtos.LessonEmail
                        {
                            SchoolCode = roll.SchoolCode,
                            SchoolName = roll.School.Name
                        };

                        email.Lessons.Add(new EmailDtos.LessonEmail.LessonItem { Name = description, DueDate = lesson.DueDate, OverdueSeverity = overdue });

                        schools.Add(email);
                    }
                    else
                    {
                        schoolEmail.Lessons.Add(new EmailDtos.LessonEmail.LessonItem { Name = description, DueDate = lesson.DueDate, OverdueSeverity = overdue });
                    }
                }
            }

            // Should now have a list of schools with a sublist of all the outstanding lessons and their due dates.

            // Email this list to the contacts of each school.
            foreach (var schoolItem in schools.OrderBy(email => email.SchoolName))
            {
                // who are the school contacts to send the emails to?
                var contacts = _unitOfWork.SchoolContactRoles.AllFromSchool(schoolItem.SchoolCode);

                var spt = contacts
                    .Where(role => role.Role == SchoolContactRole.SciencePrac)
                    .Select(role => role.SchoolContact)
                    .Select(contact => new EmailBaseClass.Recipient { Name = contact.DisplayName, Email = contact.EmailAddress })
                    .ToList();

                var acc = contacts
                    .Where(role => role.Role == SchoolContactRole.Coordinator)
                    .Select(role => role.SchoolContact)
                    .Select(contact => new EmailBaseClass.Recipient { Name = contact.DisplayName, Email = contact.EmailAddress })
                    .ToList();

                var principal = contacts
                    .Where(role => role.Role == SchoolContactRole.Principal)
                    .Select(role => role.SchoolContact)
                    .Select(contact => new EmailBaseClass.Recipient { Name = contact.DisplayName, Email = contact.EmailAddress })
                    .ToList();

                var school = new LessonMissedNotificationEmail.Recipient { Name = contacts.First().School.Name, Email = contacts.First().School.EmailAddress };

                // Break these down into the outstanding time
                // Eg 1. first email after due date (SPT)
                // 2. second email a week after first (SPT & ACC)
                // 3. third email a week after second (SPT, ACC, & SCHOOL)
                // 4. fourth email a week after third (SPT, ACC, SCHOOL, & PRINCIPAL)
                // 5. remaining emails sent to SPC/HT (SPC & HT)
                var firstWarning = schoolItem.Lessons.Where(lesson => lesson.OverdueSeverity == 1).ToList();
                var secondWarning = schoolItem.Lessons.Where(lesson => lesson.OverdueSeverity == 2).ToList();
                var thirdWarning = schoolItem.Lessons.Where(lesson => lesson.OverdueSeverity == 3).ToList();
                var finalWarning = schoolItem.Lessons.Where(lesson => lesson.OverdueSeverity == 4).ToList();
                var alert = schoolItem.Lessons.Where(lesson => lesson.OverdueSeverity >= 5).ToList();

                _logger.Log(LogSeverity.Information, $"Emails for {school.Name} to be delivered to:");

                foreach (var contact in spt)
                    _logger.Log(LogSeverity.Information, $" (SPT) {contact.Name} - {contact.Email}");

                foreach (var contact in acc)
                    _logger.Log(LogSeverity.Information, $" (ACC) {contact.Name} - {contact.Email}");

                _logger.Log(LogSeverity.Information, $" (SCH) {school.Name} - {school.Email}");

                foreach (var contact in principal)
                    _logger.Log(LogSeverity.Information, $" (PRN) {contact.Name} - {contact.Email}");

                _logger.Log(LogSeverity.Information, $"");

                if (firstWarning.Any())
                {
                    foreach (var lesson in firstWarning)
                        _logger.Log(LogSeverity.Information, $" (1W) {lesson.Name} - {lesson.DueDate.ToShortDateString()}");

                    var notification = new LessonMissedNotificationEmail
                    {
                        SchoolName = school.Name,
                        NotificationType = LessonMissedNotificationEmail.NotificationSequence.First,
                        Lessons = firstWarning,
                        Recipients = spt
                    };

                    await _emailService.SendLessonMissedEmail(notification);
                }

                if (secondWarning.Any())
                {
                    foreach (var lesson in secondWarning)
                        _logger.Log(LogSeverity.Information, $" (2W) {lesson.Name} - {lesson.DueDate.ToShortDateString()}");

                    var notification = new LessonMissedNotificationEmail
                    {
                        SchoolName = school.Name,
                        NotificationType = LessonMissedNotificationEmail.NotificationSequence.Second,
                        Lessons = secondWarning,
                        Recipients = spt.Concat(acc).ToList()
                    };

                    await _emailService.SendLessonMissedEmail(notification);
                }

                if (thirdWarning.Any())
                {
                    foreach (var lesson in thirdWarning)
                        _logger.Log(LogSeverity.Information, $" (3W) {lesson.Name} - {lesson.DueDate.ToShortDateString()}");

                    var notification = new LessonMissedNotificationEmail
                    {
                        SchoolName = school.Name,
                        NotificationType = LessonMissedNotificationEmail.NotificationSequence.Third,
                        Lessons = thirdWarning,
                        Recipients = spt.Concat(acc).Concat(new List<EmailBaseClass.Recipient> { school }).ToList()
                    };

                    await _emailService.SendLessonMissedEmail(notification);
                }

                if (finalWarning.Any())
                {
                    foreach (var lesson in finalWarning)
                        _logger.Log(LogSeverity.Information, $" (4W) {lesson.Name} - {lesson.DueDate.ToShortDateString()}");

                    var notification = new LessonMissedNotificationEmail
                    {
                        SchoolName = school.Name,
                        NotificationType = LessonMissedNotificationEmail.NotificationSequence.Final,
                        Lessons = finalWarning,
                        Recipients = spt.Concat(acc).Concat(new List<EmailBaseClass.Recipient> { school }).Concat(principal).ToList()
                    };

                    await _emailService.SendLessonMissedEmail(notification);
                }

                if (alert.Any())
                {
                    foreach (var lesson in alert)
                        _logger.Log(LogSeverity.Information, $" (FW) {lesson.Name} - {lesson.DueDate.ToShortDateString()}");

                    var notification = new LessonMissedNotificationEmail
                    {
                        SchoolName = school.Name,
                        NotificationType = LessonMissedNotificationEmail.NotificationSequence.Alert,
                        Lessons = alert,
                        Recipients = facultyContacts
                    };

                    await _emailService.SendLessonMissedEmail(notification);
                }
            }

            var logNotification = new ServiceLogEmail
            {
                Log = _logger.GetLogHistory(),
                Source = Assembly.GetEntryAssembly().GetName().Name,
                Recipients = facultyContacts
            };

            await _emailService.SendServiceLogEmail(logNotification);
        }
    }
}

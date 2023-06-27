namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.DTOs;
using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Constellation.Infrastructure.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

public class LessonNotificationsJob : ILessonNotificationsJob, IHangfireJob
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailService _emailService;
    private readonly AppConfiguration _configuration;
    private readonly ILogHandler<ILessonNotificationsJob> _logger;

    public LessonNotificationsJob(
        ILessonRepository lessonRepository,
        ISchoolRepository schoolRepository,
        ICourseRepository courseRepository,
        ISchoolContactRepository contactRepository,
        IEmailService emailService,
        IOptions<AppConfiguration> configuration,
        ILogHandler<ILessonNotificationsJob> logger)
    {
        _lessonRepository = lessonRepository;
        _schoolRepository = schoolRepository;
        _courseRepository = courseRepository;
        _contactRepository = contactRepository;
        _emailService = emailService;
        _configuration = configuration.Value;
        _logger = logger;
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        _logger.Log(LogSeverity.Information, $"Service started.");

        string coordinatorEmail = _configuration.Lessons.CoordinatorEmail;
        List<string> headTeacherEmails = _configuration.Lessons.HeadTeacherEmail;
        List<EmailRecipient> facultyContacts = new();
        
        foreach (string headTeacher in headTeacherEmails)
        {
            Result<EmailRecipient> htResult = EmailRecipient.Create(headTeacher, headTeacher);

            if (htResult.IsSuccess && facultyContacts.All(contact => contact.Email != htResult.Value.Email))
                facultyContacts.Add(htResult.Value);
        }

        Result<EmailRecipient> coordinatorResult = EmailRecipient.Create(_configuration.Lessons.CoordinatorName, coordinatorEmail);

        if (coordinatorResult.IsSuccess && facultyContacts.All(contact => contact.Email != coordinatorResult.Value.Email))
            facultyContacts.Add(coordinatorResult.Value);


        List<School> schools = await _schoolRepository.GetWithCurrentStudents(cancellationToken);

        foreach (School school in schools)
        {
            List<Lesson> lessons = await _lessonRepository.GetOverdueForSchool(school.Code, cancellationToken);

            if (lessons is null || lessons.Count == 0)
                continue;

            EmailDtos.LessonEmail schoolEmailDto = new()
            {
                SchoolCode = school.Code,
                SchoolName = school.Name
            };

            foreach (Lesson lesson in lessons)
            {
                Course course = await _courseRepository.GetByLessonId(lesson.Id, cancellationToken);

                if (course is null)
                    continue;

                string description = $"{course.Grade} {lesson.Name}";

                if (course.Grade == Grade.Y11 || course.Grade == Grade.Y12)
                {
                    description = $"{course.Grade} {course.Name} {lesson.Name}";
                }

                int overdue = (DateTime.Today.Subtract(lesson.DueDate).Days / 7) + 1;

                schoolEmailDto.Lessons.Add(
                    new EmailDtos.LessonEmail.LessonItem 
                    { 
                        Name = description, 
                        DueDate = lesson.DueDate, 
                        OverdueSeverity = overdue 
                    });
            }

            // who are the school contacts to send the emails to?
            List<SchoolContact> contacts = await _contactRepository.GetWithRolesBySchool(school.Code, cancellationToken);

            List<EmailRecipient> sptRecipients = new();
            List<EmailRecipient> accRecipients = new();
            List<EmailRecipient> principalRecipients = new();

            foreach (SchoolContact contact in contacts.Where(contact => contact.Assignments.Any(role => role.Role == SchoolContactRole.SciencePrac && !role.IsDeleted)))
            {
                Result<EmailRecipient> result = EmailRecipient.Create(contact.DisplayName, contact.EmailAddress);

                if (result.IsSuccess && sptRecipients.All(entry => entry.Email != result.Value.Email))
                    sptRecipients.Add(result.Value);
            }

            foreach (SchoolContact contact in contacts.Where(contact => contact.Assignments.Any(role => role.Role == SchoolContactRole.Coordinator && !role.IsDeleted)))
            {
                Result<EmailRecipient> result = EmailRecipient.Create(contact.DisplayName, contact.EmailAddress);

                if (result.IsSuccess && accRecipients.All(entry => entry.Email != result.Value.Email))
                    accRecipients.Add(result.Value);
            }

            foreach (SchoolContact contact in contacts.Where(contact => contact.Assignments.Any(role => role.Role == SchoolContactRole.Principal && !role.IsDeleted)))
            {
                Result<EmailRecipient> result = EmailRecipient.Create(contact.DisplayName, contact.EmailAddress);

                if (result.IsSuccess && principalRecipients.All(entry => entry.Email != result.Value.Email))
                    principalRecipients.Add(result.Value);
            }

            Result<EmailRecipient> schoolResult = EmailRecipient.Create(school.Name, school.EmailAddress);
            if (!schoolResult.IsSuccess)
                continue;
            
            // Break these down into the outstanding time
            // Eg 1. first email after due date (SPT & ACC)
            // 2. second email a week after first (SPT, ACC) (rpt)
            // 3. third email a week after second (SPT, ACC, & SCHOOL)
            // 4. fourth email a week after third (SPT, ACC, SCHOOL, & PRINCIPAL)
            // 5. remaining emails sent to SPC/HT (SPC & HT)
            var firstWarning = schoolEmailDto.Lessons.Where(lesson => lesson.OverdueSeverity == 1).ToList();
            var secondWarning = schoolEmailDto.Lessons.Where(lesson => lesson.OverdueSeverity == 2).ToList();
            var thirdWarning = schoolEmailDto.Lessons.Where(lesson => lesson.OverdueSeverity == 3).ToList();
            var finalWarning = schoolEmailDto.Lessons.Where(lesson => lesson.OverdueSeverity == 4).ToList();
            var alert = schoolEmailDto.Lessons.Where(lesson => lesson.OverdueSeverity >= 5).ToList();

            _logger.Log(LogSeverity.Information, $"");
            _logger.Log(LogSeverity.Information, $"Emails for {school.Name} to be delivered to:");

            foreach (var contact in sptRecipients)
                _logger.Log(LogSeverity.Information, $" (SPT) {contact.Name} - {contact.Email}");

            foreach (var contact in accRecipients)
                _logger.Log(LogSeverity.Information, $" (ACC) {contact.Name} - {contact.Email}");

            _logger.Log(LogSeverity.Information, $" (SCH) {schoolResult.Value.Name} - {schoolResult.Value.Email}");

            foreach (var contact in principalRecipients)
                _logger.Log(LogSeverity.Information, $" (PRN) {contact.Name} - {contact.Email}");

            _logger.Log(LogSeverity.Information, $"");

            if (firstWarning.Any())
            {
                foreach (EmailDtos.LessonEmail.LessonItem lesson in firstWarning)
                    _logger.Log(LogSeverity.Information, $" (1W) {lesson.Name} - {lesson.DueDate.ToShortDateString()}");

                LessonMissedNotificationEmail notification = new()
                {
                    SchoolName = school.Name,
                    NotificationType = LessonMissedNotificationEmail.NotificationSequence.First,
                    Lessons = firstWarning,
                    Recipients = sptRecipients
                        .Concat(accRecipients)
                        .Distinct()
                        .ToList()
                };

                await _emailService.SendLessonMissedEmail(notification);
            }

            if (secondWarning.Any())
            {
                foreach (EmailDtos.LessonEmail.LessonItem lesson in secondWarning)
                    _logger.Log(LogSeverity.Information, $" (2W) {lesson.Name} - {lesson.DueDate.ToShortDateString()}");

                LessonMissedNotificationEmail notification = new()
                {
                    SchoolName = school.Name,
                    NotificationType = LessonMissedNotificationEmail.NotificationSequence.Second,
                    Lessons = secondWarning,
                    Recipients = sptRecipients
                        .Concat(accRecipients)
                        .Concat(new List<EmailRecipient> { schoolResult.Value })
                        .Distinct()
                        .ToList()
                };

                await _emailService.SendLessonMissedEmail(notification);
            }

            if (thirdWarning.Any())
            {
                foreach (EmailDtos.LessonEmail.LessonItem lesson in thirdWarning)
                    _logger.Log(LogSeverity.Information, $" (3W) {lesson.Name} - {lesson.DueDate.ToShortDateString()}");

                LessonMissedNotificationEmail notification = new()
                {
                    SchoolName = school.Name,
                    NotificationType = LessonMissedNotificationEmail.NotificationSequence.Third,
                    Lessons = thirdWarning,
                    Recipients = sptRecipients
                        .Concat(accRecipients)
                        .Concat(new List<EmailRecipient> { schoolResult.Value })
                        .Concat(principalRecipients)
                        .Distinct()
                        .ToList()
                };

                await _emailService.SendLessonMissedEmail(notification);
            }

            if (finalWarning.Any())
            {
                foreach (EmailDtos.LessonEmail.LessonItem lesson in finalWarning)
                    _logger.Log(LogSeverity.Information, $" (4W) {lesson.Name} - {lesson.DueDate.ToShortDateString()}");

                LessonMissedNotificationEmail notification = new()
                {
                    SchoolName = school.Name,
                    NotificationType = LessonMissedNotificationEmail.NotificationSequence.Final,
                    Lessons = finalWarning,
                    Recipients = sptRecipients
                        .Concat(accRecipients)
                        .Concat(new List<EmailRecipient> { schoolResult.Value })
                        .Concat(principalRecipients)
                        .Distinct()
                        .ToList()
                };

                await _emailService.SendLessonMissedEmail(notification);
            }

            if (alert.Any())
            {
                foreach (EmailDtos.LessonEmail.LessonItem lesson in alert)
                    _logger.Log(LogSeverity.Information, $" (FW) {lesson.Name} - {lesson.DueDate.ToShortDateString()}");

                LessonMissedNotificationEmail notification = new()
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

        if (cancellationToken.IsCancellationRequested)
            return;

        await _emailService.SendServiceLogEmail(logNotification);
    }
}

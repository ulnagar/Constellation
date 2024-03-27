namespace Constellation.Infrastructure.Jobs;

using Application.DTOs;
using Application.DTOs.EmailRequests;
using Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Jobs;
using Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Core.Abstractions.Repositories;
using Core.Enums;
using Core.Models;
using Core.Models.SchoolContacts;
using Core.Models.SciencePracs;
using Core.Models.Subjects;
using Core.Shared;
using Core.ValueObjects;
using Services;
using Core.Abstractions.Clock;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.Subjects.Repositories;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

internal sealed class LessonNotificationsJob : ILessonNotificationsJob
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppConfiguration _configuration;
    private readonly ILogHandler<ILessonNotificationsJob> _logger;

    public LessonNotificationsJob(
        ILessonRepository lessonRepository,
        ISchoolRepository schoolRepository,
        ICourseRepository courseRepository,
        ISchoolContactRepository contactRepository,
        IEmailService emailService,
        IOptions<AppConfiguration> configuration,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogHandler<ILessonNotificationsJob> logger)
    {
        _lessonRepository = lessonRepository;
        _schoolRepository = schoolRepository;
        _courseRepository = courseRepository;
        _contactRepository = contactRepository;
        _emailService = emailService;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
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

        foreach (School school in schools.OrderBy(school => school.Name))
        {
            List<SciencePracLesson> lessons = await _lessonRepository.GetAllForSchool(school.Code, cancellationToken);

            lessons = lessons
                .Where(lesson =>
                    lesson.Rolls.Any(roll =>
                        roll.SchoolCode == school.Code &&
                        roll.Status == LessonStatus.Active) &&
                    lesson.DueDate < _dateTime.Today)
                .ToList();

            if (lessons is null || lessons.Count == 0)
                continue;

            List<LessonEmail.LessonItem> lessonItems = new();

            foreach (SciencePracLesson lesson in lessons)
            {
                Course course = await _courseRepository.GetByLessonId(lesson.Id, cancellationToken);

                if (course is null)
                    continue;

                SciencePracRoll roll = lesson.Rolls.SingleOrDefault(roll => roll.SchoolCode == school.Code);

                if (roll is null)
                    continue;

                string description = $"{course.Grade} {lesson.Name}";

                if (course.Grade == Grade.Y11 || course.Grade == Grade.Y12)
                    description = $"{course.Grade} {course.Name} {lesson.Name}";

                lessonItems.Add(
                    new LessonEmail.LessonItem(
                        lesson.Id,
                        roll.Id,
                        description,
                        lesson.DueDate, 
                        roll.NotificationCount));
            }

            LessonEmail schoolEmailDto = new(
                school.Code,
                school.Name,
                lessonItems);

            // who are the school contacts to send the emails to?
            List<SchoolContact> contacts = await _contactRepository.GetWithRolesBySchool(school.Code, cancellationToken);

            Result<EmailRecipient> schoolResult = EmailRecipient.Create(school.Name, school.EmailAddress);
            if (!schoolResult.IsSuccess)
            {
                _logger.Log(LogSeverity.Error, $"Could not find School Email for {school.Name}");
                continue;
            }

            List<EmailRecipient> sptRecipients = new();
            List<EmailRecipient> accRecipients = new();
            List<EmailRecipient> principalRecipients = new();

            foreach (SchoolContact contact in contacts.Where(contact => contact.Assignments.Any(role => role.Role == SchoolContactRole.SciencePrac && !role.IsDeleted)))
            {
                Result<EmailRecipient> result = EmailRecipient.Create(contact.DisplayName, contact.EmailAddress);

                if (result.IsSuccess && sptRecipients.All(entry => entry.Email != result.Value.Email))
                    sptRecipients.Add(result.Value);
            }

            if (!sptRecipients.Any())
                sptRecipients.Add(schoolResult.Value);

            foreach (SchoolContact contact in contacts.Where(contact => contact.Assignments.Any(role => role.Role == SchoolContactRole.Coordinator && !role.IsDeleted)))
            {
                Result<EmailRecipient> result = EmailRecipient.Create(contact.DisplayName, contact.EmailAddress);

                if (result.IsSuccess && accRecipients.All(entry => entry.Email != result.Value.Email))
                    accRecipients.Add(result.Value);
            }

            if (!accRecipients.Any())
                accRecipients.Add(schoolResult.Value);

            foreach (SchoolContact contact in contacts.Where(contact => contact.Assignments.Any(role => role.Role == SchoolContactRole.Principal && !role.IsDeleted)))
            {
                Result<EmailRecipient> result = EmailRecipient.Create(contact.DisplayName, contact.EmailAddress);

                if (result.IsSuccess && principalRecipients.All(entry => entry.Email != result.Value.Email))
                    principalRecipients.Add(result.Value);
            }

            if (!principalRecipients.Any())
                principalRecipients.Add(schoolResult.Value);
            
            // Break these down into the outstanding time
            // Eg 1. first email after due date (SPT & ACC)
            // 2. second email a week after first (SPT, ACC) (rpt)
            // 3. third email a week after second (SPT, ACC, & SCHOOL)
            // 4. fourth email a week after third (SPT, ACC, SCHOOL, & PRINCIPAL)
            // 5. remaining emails sent to SPC/HT (SPC & HT)
            List<LessonEmail.LessonItem> firstWarning = schoolEmailDto.Lessons.Where(lesson => lesson.NotificationCount == 0).ToList();
            List<LessonEmail.LessonItem> secondWarning = schoolEmailDto.Lessons.Where(lesson => lesson.NotificationCount == 1).ToList();
            List<LessonEmail.LessonItem> thirdWarning = schoolEmailDto.Lessons.Where(lesson => lesson.NotificationCount == 2).ToList();
            List<LessonEmail.LessonItem> finalWarning = schoolEmailDto.Lessons.Where(lesson => lesson.NotificationCount == 3).ToList();
            List<LessonEmail.LessonItem> alert = schoolEmailDto.Lessons.Where(lesson => lesson.NotificationCount >= 4).ToList();

            _logger.Log(LogSeverity.Information, string.Empty);
            _logger.Log(LogSeverity.Information, $"Emails for {school.Name} to be delivered to:");

            foreach (EmailRecipient contact in sptRecipients)
                _logger.Log(LogSeverity.Information, $" (SPT) {contact.Name} - {contact.Email}");

            foreach (EmailRecipient contact in accRecipients)
                _logger.Log(LogSeverity.Information, $" (ACC) {contact.Name} - {contact.Email}");

            _logger.Log(LogSeverity.Information, $" (SCH) {schoolResult.Value.Name} - {schoolResult.Value.Email}");

            foreach (EmailRecipient contact in principalRecipients)
                _logger.Log(LogSeverity.Information, $" (PRN) {contact.Name} - {contact.Email}");

            _logger.Log(LogSeverity.Information, string.Empty);

            if (firstWarning.Any())
            {
                foreach (LessonEmail.LessonItem item in firstWarning)
                    _logger.Log(LogSeverity.Information, $" (1W) {item.Name} - {item.DueDate.ToShortDateString()}");

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

                foreach (LessonEmail.LessonItem item in firstWarning)
                {
                    SciencePracRoll roll = lessons
                        .Where(lesson => lesson.Id == item.LessonId)
                        .SelectMany(lesson => lesson.Rolls)
                        .First(roll => roll.Id == item.RollId);

                    roll.IncrementNotificationCount();
                }
            }

            if (secondWarning.Any())
            {
                foreach (LessonEmail.LessonItem lesson in secondWarning)
                    _logger.Log(LogSeverity.Information, $" (2W) {lesson.Name} - {lesson.DueDate.ToShortDateString()}");

                LessonMissedNotificationEmail notification = new()
                {
                    SchoolName = school.Name,
                    NotificationType = LessonMissedNotificationEmail.NotificationSequence.Second,
                    Lessons = secondWarning,
                    Recipients = sptRecipients
                        .Concat(accRecipients)
                        .Distinct()
                        .ToList()
                };

                await _emailService.SendLessonMissedEmail(notification);

                foreach (LessonEmail.LessonItem item in secondWarning)
                {
                    SciencePracRoll roll = lessons
                        .Where(lesson => lesson.Id == item.LessonId)
                        .SelectMany(lesson => lesson.Rolls)
                        .First(roll => roll.Id == item.RollId);

                    roll.IncrementNotificationCount();
                }
            }

            if (thirdWarning.Any())
            {
                foreach (LessonEmail.LessonItem lesson in thirdWarning)
                    _logger.Log(LogSeverity.Information, $" (3W) {lesson.Name} - {lesson.DueDate.ToShortDateString()}");

                LessonMissedNotificationEmail notification = new()
                {
                    SchoolName = school.Name,
                    NotificationType = LessonMissedNotificationEmail.NotificationSequence.Third,
                    Lessons = thirdWarning,
                    Recipients = sptRecipients
                        .Concat(accRecipients)
                        .Concat(new List<EmailRecipient> { schoolResult.Value })
                        .Distinct()
                        .ToList()
                };

                await _emailService.SendLessonMissedEmail(notification);

                foreach (LessonEmail.LessonItem item in thirdWarning)
                {
                    SciencePracRoll roll = lessons
                        .Where(lesson => lesson.Id == item.LessonId)
                        .SelectMany(lesson => lesson.Rolls)
                        .First(roll => roll.Id == item.RollId);

                    roll.IncrementNotificationCount();
                }
            }

            if (finalWarning.Any())
            {
                foreach (LessonEmail.LessonItem lesson in finalWarning)
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

                foreach (LessonEmail.LessonItem item in finalWarning)
                {
                    SciencePracRoll roll = lessons
                        .Where(lesson => lesson.Id == item.LessonId)
                        .SelectMany(lesson => lesson.Rolls)
                        .First(roll => roll.Id == item.RollId);

                    roll.IncrementNotificationCount();
                }
            }

            if (alert.Any())
            {
                foreach (LessonEmail.LessonItem lesson in alert)
                    _logger.Log(LogSeverity.Information, $" (FW) {lesson.Name} - {lesson.DueDate.ToShortDateString()}");

                LessonMissedNotificationEmail notification = new()
                {
                    SchoolName = school.Name,
                    NotificationType = LessonMissedNotificationEmail.NotificationSequence.Alert,
                    Lessons = alert,
                    Recipients = facultyContacts
                };

                await _emailService.SendLessonMissedEmail(notification);

                foreach (LessonEmail.LessonItem item in firstWarning)
                {
                    SciencePracRoll roll = lessons
                        .Where(lesson => lesson.Id == item.LessonId)
                        .SelectMany(lesson => lesson.Rolls)
                        .First(roll => roll.Id == item.RollId);

                    roll.IncrementNotificationCount();
                }
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        ServiceLogEmail logNotification = new()
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

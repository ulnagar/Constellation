namespace Constellation.Application.SciencePracs.SendLessonNotification;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Errors;
using Core.Errors;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.SciencePracs.Errors;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Core.ValueObjects;
using DTOs;
using DTOs.EmailRequests;
using Interfaces.Configuration;
using Interfaces.Repositories;
using Interfaces.Services;
using Microsoft.Extensions.Options;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SendLessonNotificationCommandHandler
    : ICommandHandler<SendLessonNotificationCommand>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppConfiguration _configuration;
    private readonly ILogger _logger;
    
    public SendLessonNotificationCommandHandler(
        ILessonRepository lessonRepository,
        ISchoolRepository schoolRepository,
        ISchoolContactRepository contactRepository,
        ICourseRepository courseRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IOptions<AppConfiguration> configuration,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _schoolRepository = schoolRepository;
        _contactRepository = contactRepository;
        _courseRepository = courseRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _configuration = configuration.Value;
        _logger = logger.ForContext<SendLessonNotificationCommand>();
    }

    public async Task<Result> Handle(SendLessonNotificationCommand request, CancellationToken cancellationToken)
    {
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

        SciencePracLesson lesson = await _lessonRepository.GetById(request.LessonId, cancellationToken);

        if (lesson is null)
        {
            _logger.Warning("Could not find a Science Prac Lesson with Id {id}", request.LessonId);

            return Result.Failure(SciencePracLessonErrors.NotFound(request.LessonId));
        }

        SciencePracRoll roll = lesson.Rolls.SingleOrDefault(roll => roll.Id == request.RollId);

        if (roll is null)
        {
            _logger.Warning("Could not find Science Prac Roll with Id {id}", request.RollId);

            return Result.Failure(SciencePracRollErrors.NotFound(request.RollId));
        }

        School school = await _schoolRepository.GetById(roll.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger.Warning("Could not find School with Code {code}", roll.SchoolCode);

            return Result.Failure(DomainErrors.Partners.School.NotFound(roll.SchoolCode));
        }

        List<LessonEmail.LessonItem> lessonItems = new();

        Course course = await _courseRepository.GetByLessonId(lesson.Id, cancellationToken);

        if (course is null)
        {
            _logger.Warning("Could not find a Course linked with the Lesson Id {id}", lesson.Id);

            return Result.Failure(CourseErrors.NoneFound);
        }

        string description = $"{course.Grade} {lesson.Name}";

        if (course.Grade is Grade.Y11 or Grade.Y12)
            description = $"{course.Grade} {course.Name} {lesson.Name}";

        lessonItems.Add(
            new(
                lesson.Id,
                roll.Id,
                description,
                lesson.DueDate,
                roll.NotificationCount));

        // who are the school contacts to send the emails to?
        List<SchoolContact> contacts = await _contactRepository.GetWithRolesBySchool(school.Code, cancellationToken);

        List<EmailRecipient> sptRecipients = new();
        List<EmailRecipient> accRecipients = new();

        foreach (SchoolContact contact in contacts.Where(contact => contact.Assignments.Any(role => role.Role == Position.SciencePracticalTeacher && !role.IsDeleted)))
        {
            Result<EmailRecipient> result = contact.GetEmailRecipient();

            if (result.IsSuccess && sptRecipients.All(entry => entry.Email != result.Value.Email))
                sptRecipients.Add(result.Value);
        }

        foreach (SchoolContact contact in contacts.Where(contact => contact.Assignments.Any(role => role.Role == Position.Coordinator && !role.IsDeleted)))
        {
            Result<EmailRecipient> result = contact.GetEmailRecipient();

            if (result.IsSuccess && accRecipients.All(entry => entry.Email != result.Value.Email))
                accRecipients.Add(result.Value);
        }

        Result<EmailRecipient> schoolResult = EmailRecipient.Create(school.Name, school.EmailAddress);
        if (schoolResult.IsFailure)
        {
            _logger.Warning("Could not create EmailRecipient based on school {@school}", school);

            return Result.Failure(schoolResult.Error);
        }

        LessonMissedNotificationEmail notification = new()
        {
            SchoolName = school.Name,
            Lessons = lessonItems,
            NotificationType = roll.NotificationCount switch
            {
                0 => LessonMissedNotificationEmail.NotificationSequence.First,
                1 => LessonMissedNotificationEmail.NotificationSequence.Second,
                2 => LessonMissedNotificationEmail.NotificationSequence.Third,
                3 => LessonMissedNotificationEmail.NotificationSequence.Final,
                >= 4 => LessonMissedNotificationEmail.NotificationSequence.Alert,
                _ => LessonMissedNotificationEmail.NotificationSequence.First
            },
            Recipients = roll.NotificationCount switch
            {
                0 or 1 => sptRecipients.Concat(accRecipients).Distinct().ToList(),
                2 or 3 => sptRecipients.Concat(accRecipients).Concat(new List<EmailRecipient> { schoolResult.Value }).Distinct().ToList(),
                >= 4 => facultyContacts,
                _ => sptRecipients.Concat(accRecipients).Distinct().ToList()
            }
        };

        await _emailService.SendLessonMissedEmail(notification);

        if (request.ShouldIncrementCount)
        {
            roll.IncrementNotificationCount();
        }
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
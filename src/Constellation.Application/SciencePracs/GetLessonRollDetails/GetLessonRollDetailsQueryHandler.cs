namespace Constellation.Application.SciencePracs.GetLessonRollDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetLessonRollDetailsQueryHandler
    : IQueryHandler<GetLessonRollDetailsQuery, LessonRollDetailsResponse>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ILogger _logger;

    public GetLessonRollDetailsQueryHandler(
        ILessonRepository lessonRepository,
        ISchoolRepository schoolRepository,
        IStudentRepository studentRepository,
        ISchoolContactRepository contactRepository,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _schoolRepository = schoolRepository;
        _studentRepository = studentRepository;
        _contactRepository = contactRepository;
        _logger = logger.ForContext<GetLessonRollDetailsQuery>();
    }

    public async Task<Result<LessonRollDetailsResponse>> Handle(GetLessonRollDetailsQuery request, CancellationToken cancellationToken)
    {
        SciencePracLesson lesson = await _lessonRepository.GetById(request.LessonId, cancellationToken);

        if (lesson is null)
        {
            _logger.Warning("Could not find a Science Prac Lesson with the Id {id}", request.LessonId);

            return Result.Failure<LessonRollDetailsResponse>(DomainErrors.SciencePracs.Lesson.NotFound(request.LessonId));
        }

        SciencePracRoll roll = lesson.Rolls.SingleOrDefault(roll => roll.Id == request.RollId);

        if (roll is null)
        {
            _logger.Warning("Could not find a Science Prac Roll with the Id {id}", request.RollId);

            return Result.Failure<LessonRollDetailsResponse>(DomainErrors.SciencePracs.Roll.NotFound(request.RollId));
        }

        List<LessonRollDetailsResponse.AttendanceRecord> attendanceRecords = new();

        foreach (SciencePracAttendance entry in roll.Attendance)
        {
            Student student = await _studentRepository.GetById(entry.StudentId, cancellationToken);

            if (student is null)
                continue;

            attendanceRecords.Add(new(
                entry.Id,
                entry.StudentId,
                student.GetName(),
                entry.Present));
        }

        School school = await _schoolRepository.GetById(roll.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger.Warning("Could not locate school with Id {code}", roll.SchoolCode);

            return Result.Failure<LessonRollDetailsResponse>(DomainErrors.Partners.School.NotFound(roll.SchoolCode));
        }

        LessonRollDetailsResponse.Contact contactDetails = null;

        if (roll.SchoolContactId.HasValue)
        {
            SchoolContact contact = await _contactRepository.GetById(roll.SchoolContactId.Value, cancellationToken);

            if (contact is not null)
            {
                Result<Name> contactName = Name.Create(contact.FirstName, string.Empty, contact.LastName);

                if (contactName.IsFailure)
                {
                    _logger
                        .ForContext(nameof(SchoolContact), contact, true)
                        .Warning("Could not create Name for contact with Id {contactId}", contact.Id);

                    return Result.Failure<LessonRollDetailsResponse>(contactName.Error);
                }

                Result<EmailAddress> emailAddress = EmailAddress.Create(contact.EmailAddress);

                if (emailAddress.IsFailure)
                {
                    _logger
                        .ForContext(nameof(SchoolContact), contact, true)
                        .Warning("Could not create EmailAddress for contact with Id {contactId}", contact.Id);

                    return Result.Failure<LessonRollDetailsResponse>(emailAddress.Error);
                }

                contactDetails = new(
                    contact.Id,
                    contactName.Value,
                    emailAddress.Value);
            }
        }
        else if (!string.IsNullOrWhiteSpace(roll.SubmittedBy))
        {
            Result<Name> contactName = Name.CreateMononym(roll.SubmittedBy);

            if (contactName.IsFailure)
            {
                _logger
                    .Warning("Could not create Name from roll submitted by field");

                return Result.Failure<LessonRollDetailsResponse>(contactName.Error);
            }

            contactDetails = new(
                0,
                contactName.Value,
                EmailAddress.None);
        }

        LessonRollDetailsResponse response = new(
            lesson.Id,
            lesson.Name,
            lesson.DueDate,
            school.Code,
            school.Name,
            contactDetails,
            roll.LessonDate,
            roll.SubmittedDate,
            roll.Comment,
            roll.Status,
            roll.NotificationCount,
            attendanceRecords);

        return response;
    }
}

namespace Constellation.Application.SciencePracs.GetLessonRollDetails;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Errors;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.Students.Identifiers;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
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
                student.StudentReferenceNumber,
                student.Name,
                entry.Present));
        }

        School school = await _schoolRepository.GetById(roll.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger.Warning("Could not locate school with Id {code}", roll.SchoolCode);

            return Result.Failure<LessonRollDetailsResponse>(DomainErrors.Partners.School.NotFound(roll.SchoolCode));
        }

        LessonRollDetailsResponse.Contact contactDetails = null;

        if (!string.IsNullOrWhiteSpace(roll.SubmittedBy))
        {
            SchoolContact contact = roll.SubmittedBy.Contains('@')
                ? await _contactRepository.GetWithRolesByEmailAddress(roll.SubmittedBy, cancellationToken)
                : await _contactRepository.GetByNameAndSchool(roll.SubmittedBy, roll.SchoolCode, cancellationToken);

            if (contact is null)
            {
                Result<Name> contactName = Name.Create(roll.SubmittedBy, string.Empty, "-");

                if (contactName.IsFailure)
                {
                    _logger
                        .Warning("Could not create Name from roll submitted by field");

                    return Result.Failure<LessonRollDetailsResponse>(contactName.Error);
                }

                Result<EmailAddress> contactEmail = EmailAddress.Create(roll.SubmittedBy);

                if (contactEmail.IsFailure)
                {
                    _logger
                        .Warning("Could not create EmailAddress from roll submitted by field");

                    return Result.Failure<LessonRollDetailsResponse>(contactEmail.Error);
                }

                contactDetails = new(
                    SchoolContactId.Empty,
                    contactName.Value,
                    contactEmail.Value);
            }
            else
            {
                Result<Name> contactName = Name.Create(contact.FirstName, string.Empty, contact.LastName);

                if (contactName.IsFailure)
                {
                    _logger
                        .Warning("Could not create Name from roll submitted by field");

                    return Result.Failure<LessonRollDetailsResponse>(contactName.Error);
                }

                Result<EmailAddress> contactEmail = EmailAddress.Create(contact.EmailAddress);

                if (contactEmail.IsFailure)
                {
                    _logger
                        .Warning("Could not create EmailAddress from roll submitted by field");

                    return Result.Failure<LessonRollDetailsResponse>(contactEmail.Error);
                }

                contactDetails = new(
                    contact.Id,
                    contactName.Value,
                    contactEmail.Value);
            }
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

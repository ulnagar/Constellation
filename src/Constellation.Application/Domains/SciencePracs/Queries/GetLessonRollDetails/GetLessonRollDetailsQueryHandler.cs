namespace Constellation.Application.Domains.SciencePracs.Queries.GetLessonRollDetails;

using Abstractions.Messaging;
using Constellation.Application.Domains.SchoolContacts.Commands.CreateContact;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Models;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.SciencePracs;
using Core.Models.SciencePracs.Errors;
using Core.Models.Students;
using Core.Models.Students.Repositories;
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

            return Result.Failure<LessonRollDetailsResponse>(SciencePracLessonErrors.NotFound(request.LessonId));
        }

        SciencePracRoll roll = lesson.Rolls.SingleOrDefault(roll => roll.Id == request.RollId);

        if (roll is null)
        {
            _logger.Warning("Could not find a Science Prac Roll with the Id {id}", request.RollId);

            return Result.Failure<LessonRollDetailsResponse>(SciencePracRollErrors.NotFound(request.RollId));
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
            SchoolContact? contact = null;

            if (roll.SubmittedBy.Contains('@'))
            {
                Result<EmailAddress> emailAddress = EmailAddress.Create(roll.SubmittedBy);

                if (emailAddress.IsFailure)
                {
                    _logger
                        .ForContext(nameof(GetLessonRollDetailsQuery), request, true)
                        .ForContext(nameof(Error), emailAddress.Error, true)
                        .Warning("Failed to retrieve Lesson Roll details");

                    return Result.Failure<LessonRollDetailsResponse>(emailAddress.Error);
                }

                contact = await _contactRepository.GetWithRolesByEmailAddress(emailAddress.Value, cancellationToken);
            }
            else
            {
                contact = await _contactRepository.GetByNameAndSchool(roll.SubmittedBy, roll.SchoolCode, cancellationToken);
            }
                
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
                contactDetails = new(
                    contact.Id,
                    contact.Name,
                    contact.EmailAddress);
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

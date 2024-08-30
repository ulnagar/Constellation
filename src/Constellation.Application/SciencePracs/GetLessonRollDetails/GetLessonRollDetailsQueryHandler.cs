namespace Constellation.Application.SciencePracs.GetLessonRollDetails;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Errors;
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
    private readonly ILogger _logger;

    public GetLessonRollDetailsQueryHandler(
        ILessonRepository lessonRepository,
        ISchoolRepository schoolRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _schoolRepository = schoolRepository;
        _studentRepository = studentRepository;
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
            string[] splitName = roll.SubmittedBy.Split(' ', 2);

            Result<Name> contactName = Name.Create(splitName.First(), string.Empty, splitName.Last());

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

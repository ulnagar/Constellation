namespace Constellation.Application.Domains.SciencePracs.Queries.GetLessonRollDetailsForSchoolsPortal;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.SciencePracs;
using Core.Models.SciencePracs.Errors;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetLessonRollDetailsForSchoolsPortalQueryHandler 
    : IQueryHandler<GetLessonRollDetailsForSchoolsPortalQuery, ScienceLessonRollDetails>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetLessonRollDetailsForSchoolsPortalQueryHandler(
        ILessonRepository lessonRepository,
        ISchoolContactRepository contactRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _contactRepository = contactRepository;
        _studentRepository = studentRepository;
        _logger = logger.ForContext<GetLessonRollDetailsForSchoolsPortalQuery>();
    }

    public async Task<Result<ScienceLessonRollDetails>> Handle(GetLessonRollDetailsForSchoolsPortalQuery request, CancellationToken cancellationToken)
    {
        SciencePracLesson lesson = await _lessonRepository.GetById(request.LessonId, cancellationToken);

        if (lesson is null)
        {
            _logger.Warning("Could not find Science Prac Lesson with Id {id}", request.LessonId);

            return Result.Failure<ScienceLessonRollDetails>(SciencePracLessonErrors.NotFound(request.LessonId));
        }

        SciencePracRoll roll = lesson.Rolls.FirstOrDefault(roll => roll.Id == request.RollId);

        if (roll is null)
        {
            _logger.Warning("Could not find Science Prac Roll with Id {id}", request.RollId);

            return Result.Failure<ScienceLessonRollDetails>(SciencePracRollErrors.NotFound(request.RollId));
        }

        ScienceLessonRollDetails response = new()
        {
            Id = roll.Id,
            LessonId = lesson.Id,
            LessonName = lesson.Name,
            LessonDueDate = lesson.DueDate.ToDateTime(TimeOnly.MinValue),
            LessonDate = roll.LessonDate?.ToDateTime(TimeOnly.MinValue),
            Comment = roll.Comment
        };

        if (roll.Status == Core.Enums.LessonStatus.Completed)
        {
            if (!string.IsNullOrWhiteSpace(roll.SubmittedBy))
            {
                SchoolContact contact = await _contactRepository.GetWithRolesByEmailAddress(roll.SubmittedBy, cancellationToken);

                if (contact is not null)
                {
                    response.SchoolContactFirstName = contact.FirstName;
                    response.SchoolContactLastName = contact.LastName;
                }
            }
        }

        foreach (SciencePracAttendance attendanceRecord in roll.Attendance)
        {
            Student student = await _studentRepository.GetById(attendanceRecord.StudentId, cancellationToken);

            if (student is null)
                continue;

            response.Attendance.Add(new()
            {
                StudentFirstName = student.Name.FirstName,
                StudentLastName = student.Name.LastName,
                Present = attendanceRecord.Present
            });
        }

        return response;
    }
}

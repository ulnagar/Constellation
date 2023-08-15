namespace Constellation.Infrastructure.Features.Portal.School.ScienceRolls.Queries;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.SciencePracs.GetLessonRollSubmitContextForSchoolsPortal;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Shared;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetLessonRollSubmitContextForSchoolsPortalQueryHandler
    : IQueryHandler<GetLessonRollSubmitContextForSchoolsPortalQuery, ScienceLessonRollForSubmit>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ILogger _logger;

    public GetLessonRollSubmitContextForSchoolsPortalQueryHandler(
        ILessonRepository lessonRepository,
        IStudentRepository studentRepository,
        ISchoolContactRepository contactRepository,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _studentRepository = studentRepository;
        _contactRepository = contactRepository;
        _logger = logger.ForContext<GetLessonRollSubmitContextForSchoolsPortalQuery>();
    }

    public async Task<Result<ScienceLessonRollForSubmit>> Handle(GetLessonRollSubmitContextForSchoolsPortalQuery request, CancellationToken cancellationToken)
    {
        SciencePracLesson lesson = await _lessonRepository.GetById(request.LessonId, cancellationToken);

        if (lesson is null)
        {
            _logger.Warning("Could not find Science Prac Lesson with the Id {id}", request.LessonId);

            return Result.Failure<ScienceLessonRollForSubmit>(DomainErrors.SciencePracs.Lesson.NotFound(request.LessonId));
        }

        SciencePracRoll roll = lesson.Rolls.FirstOrDefault(roll => roll.Id == request.RollId);

        if (roll is null)
        {
            _logger.Warning("Could not find Science Prac Roll with the Id {id}", request.RollId);

            return Result.Failure<ScienceLessonRollForSubmit>(DomainErrors.SciencePracs.Roll.NotFound(request.RollId));
        }

        ScienceLessonRollForSubmit response = new()
        {
            Id = roll.Id,
            LessonId = lesson.Id,
            LessonName = lesson.Name,
            LessonDueDate = lesson.DueDate.ToDateTime(TimeOnly.MinValue),
            LessonDate = roll.LessonDate.HasValue ? roll.LessonDate.Value.ToDateTime(TimeOnly.MinValue) : DateTime.Today,
            Comment = roll.Comment
        };

        if (roll.Status == Core.Enums.LessonStatus.Completed)
        {
            if (roll.SchoolContactId.HasValue)
            {
                SchoolContact contact = await _contactRepository.GetById(roll.SchoolContactId.Value, cancellationToken);

                if (contact is not null)
                {
                    response.TeacherName = contact.DisplayName;
                }
            }

            if (!string.IsNullOrWhiteSpace(roll.SubmittedBy))
            {
                SchoolContact contact = await _contactRepository.GetWithRolesByEmailAddress(roll.SubmittedBy, cancellationToken);

                if (contact is not null)
                {
                    response.TeacherName = contact.DisplayName;
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
                Id = attendanceRecord.Id,
                StudentId = student.StudentId,
                StudentFirstName = student.FirstName,
                StudentLastName = student.LastName,
                Present = attendanceRecord.Present
            });
        }

        return response;
    }
}

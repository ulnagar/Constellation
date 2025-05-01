namespace Constellation.Application.Domains.SciencePracs.Queries.GetRollsWithoutPresentStudents;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Enums;
using Core.Extensions;
using Core.Models;
using Core.Models.SciencePracs;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetRollsWithoutPresentStudentsQueryHandler
: IQueryHandler<GetRollsWithoutPresentStudentsQuery, List<NotPresentRollResponse>>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ISchoolRepository _schoolRepository;

    public GetRollsWithoutPresentStudentsQueryHandler(
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        ISchoolRepository schoolRepository)
    {
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _schoolRepository = schoolRepository;
    }

    public async Task<Result<List<NotPresentRollResponse>>> Handle(GetRollsWithoutPresentStudentsQuery request, CancellationToken cancellationToken)
    {
        List<NotPresentRollResponse> response = new();

        List<SciencePracLesson> lessons = await _lessonRepository.GetWithoutPresentStudents(cancellationToken);

        List<School> schools = await _schoolRepository.GetAllActive(cancellationToken);

        foreach (SciencePracLesson lesson in lessons)
        {
            Course course = await _courseRepository.GetByLessonId(lesson.Id, cancellationToken);

            if (course is null)
                continue;

            List<SciencePracRoll> rolls = lesson.Rolls.Where(roll =>
                    roll.Status == LessonStatus.Completed &&
                    roll.Attendance.All(entry => !entry.Present))
                .ToList();

            foreach (SciencePracRoll roll in rolls)
            {
                School school = schools.FirstOrDefault(entry => entry.Code == roll.SchoolCode);

                if (school is null)
                    continue;

                response.Add(new(
                    school.Code,
                    school.Name,
                    roll.LessonId,
                    roll.Id,
                    lesson.Name,
                    $"{course.Grade.AsName()} {course.Name}",
                    lesson.DueDate));
            }
        }

        return response;
    }
}

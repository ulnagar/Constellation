namespace Constellation.Application.SciencePracs.GetFilteredRollsForStudent;

using Abstractions.Messaging;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Subjects;
using Core.Abstractions.Clock;
using Core.Abstractions.Repositories;
using Core.Enums;
using Core.Shared;
using Interfaces.Repositories;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFilteredRollsForStudentQueryHandler
: IQueryHandler<GetFilteredRollsForStudentQuery, List<RollSummaryResponse>>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetFilteredRollsForStudentQueryHandler(
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task<Result<List<RollSummaryResponse>>> Handle(GetFilteredRollsForStudentQuery request, CancellationToken cancellationToken)
    {
        List<RollSummaryResponse> responses = new();

        List<SciencePracLesson> lessons = await _lessonRepository.GetAllForStudent(request.StudentId, cancellationToken);

        foreach (SciencePracLesson lesson in lessons)
        {
            IEnumerable<SciencePracRoll> studentRolls = lesson.Rolls.Where(roll => roll.Attendance.Any(entry => entry.StudentId == request.StudentId));

            foreach (SciencePracRoll roll in studentRolls)
            {
                Course course = await _courseRepository.GetByOfferingId(lesson.Offerings.First().OfferingId, cancellationToken);

                if (course is null)
                {
                    //TODO: Properly log error

                    continue;
                }

                responses.Add(new(
                    lesson.Id,
                    roll.Id,
                    lesson.Name,
                    course.ToString(),
                    lesson.DueDate,
                    roll.Status,
                    roll.Attendance.Count(entry => entry.Present),
                    roll.Attendance.Count(),
                    lesson.DueDate < _dateTime.Today && roll.Status == LessonStatus.Active));
            }
        }

        return responses;
    }
}

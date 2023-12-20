namespace Constellation.Application.SciencePracs.GetFilteredRollsForSchool;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.SciencePracs.Models;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Subjects;
using Core.Abstractions.Clock;
using Core.Enums;
using Core.Models.SciencePracs;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFilteredRollsForSchoolQueryHandlers
    : IQueryHandler<GetFilteredRollsForSchoolQuery, List<RollSummaryResponse>>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetFilteredRollsForSchoolQueryHandlers(
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _dateTime = dateTime;
        _logger = logger.ForContext<GetFilteredRollsForSchoolQuery>();
    }

    public async Task<Result<List<RollSummaryResponse>>> Handle(GetFilteredRollsForSchoolQuery request, CancellationToken cancellationToken)
    {
        List<RollSummaryResponse> responses = new();

        List<SciencePracLesson> lessons = await _lessonRepository.GetAllForSchool(request.SchoolCode, cancellationToken);

        foreach (SciencePracLesson lesson in lessons)
        {
            IEnumerable<SciencePracRoll> schoolRolls = lesson.Rolls.Where(roll => roll.SchoolCode == request.SchoolCode);

            foreach (SciencePracRoll roll in schoolRolls)
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

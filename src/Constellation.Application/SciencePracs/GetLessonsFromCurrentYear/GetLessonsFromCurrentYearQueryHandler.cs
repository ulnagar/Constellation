namespace Constellation.Application.SciencePracs.GetLessonsFromCurrentYear;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using Core.Models.Subjects.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetLessonsFromCurrentYearQueryHandler
    : IQueryHandler<GetLessonsFromCurrentYearQuery, List<LessonSummaryResponse>>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger _logger;

    public GetLessonsFromCurrentYearQueryHandler(
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _logger = logger.ForContext<GetLessonsFromCurrentYearQuery>();
    }

    public async Task<Result<List<LessonSummaryResponse>>> Handle(GetLessonsFromCurrentYearQuery request, CancellationToken cancellationToken)
    {
        List<LessonSummaryResponse> response = new();

        List<SciencePracLesson> lessons = await _lessonRepository.GetAll(cancellationToken);

        foreach (SciencePracLesson lesson in lessons)
        {
            Course course = await _courseRepository.GetByLessonId(lesson.Id, cancellationToken);

            string courseName = $"{course?.Grade} {course?.Name}";

            bool overdue = lesson.Rolls.Any(roll => roll.Status == Core.Enums.LessonStatus.Active) && lesson.DueDate < DateOnly.FromDateTime(DateTime.Today);

            LessonSummaryResponse summary = new(
                lesson.Id,
                courseName,
                lesson.Name,
                lesson.DueDate,
                lesson.Rolls.Count(roll => roll.Status != Core.Enums.LessonStatus.Active),
                lesson.Rolls.Count(),
                overdue);

            response.Add(summary);
        }

        return response;
    }
}

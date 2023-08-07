namespace Constellation.Application.SciencePracs.GetLessonsFromCurrentYear;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetLessonsFromCurrentYearQueryHandler
    : IQueryHandler<GetLessonsFromCurrentYearQuery, List<LessonSummaryResponse>>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ILogger _logger;

    public GetLessonsFromCurrentYearQueryHandler(
        ILessonRepository lessonRepository,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _logger = logger.ForContext<GetLessonsFromCurrentYearQuery>();
    }

    public async Task<Result<List<LessonSummaryResponse>>> Handle(GetLessonsFromCurrentYearQuery request, CancellationToken cancellationToken)
    {
        List<LessonSummaryResponse> response = new();

        List<SciencePracLesson> lessons = await _lessonRepository.GetAll(cancellationToken);

        foreach (SciencePracLesson lesson in lessons)
        {
            LessonSummaryResponse summary = new(
                lesson.Id,
                lesson.Name,
                lesson.DueDate,
                lesson.Rolls.Count(roll => roll.Status == Core.Enums.LessonStatus.Active),
                lesson.Rolls.Count());

            response.Add(summary);
        }

        return response;
    }
}

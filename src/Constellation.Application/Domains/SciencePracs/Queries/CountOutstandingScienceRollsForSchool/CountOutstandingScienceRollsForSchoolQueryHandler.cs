namespace Constellation.Application.Domains.SciencePracs.Queries.CountOutstandingScienceRollsForSchool;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.SciencePracs;
using Core.Abstractions.Clock;
using Core.Shared;
using System;
using System.Collections.Generic;

internal sealed class CountOutstandingScienceRollsForSchoolQueryHandler
    : IQueryHandler<CountOutstandingScienceRollsForSchoolQuery, int>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IDateTimeProvider _dateTime;

    public CountOutstandingScienceRollsForSchoolQueryHandler(
        ILessonRepository lessonRepository,
        IDateTimeProvider dateTime)
    {
        _lessonRepository = lessonRepository;
        _dateTime = dateTime;
    }

    public async Task<Result<int>> Handle(CountOutstandingScienceRollsForSchoolQuery request, CancellationToken cancellationToken)
    {
        int outstandingCount = 0;

        List<SciencePracLesson> lessons = await _lessonRepository.GetAllForSchool(request.SchoolCode, cancellationToken);

        foreach (SciencePracLesson lesson in lessons)
        {
            SciencePracRoll? roll = lesson.Rolls.FirstOrDefault(roll => roll.SchoolCode == request.SchoolCode);

            if (roll is null)
                continue;

            switch (roll.Status)
            {
                case Core.Enums.LessonStatus.Cancelled or Core.Enums.LessonStatus.Concern:
                case Core.Enums.LessonStatus.Completed:
                    continue;
            }

            DateTime dueDate = lesson.DueDate.ToDateTime(TimeOnly.MaxValue);
            if (dueDate <= _dateTime.Now)
                outstandingCount++;
        }

        return outstandingCount;
    }
}

namespace Constellation.Application.SciencePracs.GetLessonDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetLessonDetailsQueryHandler
    : IQueryHandler<GetLessonDetailsQuery, LessonDetailsResponse>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;

    public GetLessonDetailsQueryHandler(
        ILessonRepository lessonRepository,
        ICourseOfferingRepository offeringRepository,
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _offeringRepository = offeringRepository;
        _schoolRepository = schoolRepository;
        _logger = logger.ForContext<GetLessonDetailsQuery>();
    }

    public async Task<Result<LessonDetailsResponse>> Handle(GetLessonDetailsQuery request, CancellationToken cancellationToken)
    {
        SciencePracLesson lesson = await _lessonRepository.GetById(request.LessonId, cancellationToken);

        if (lesson is null)
        {
            _logger.Warning("Could not find Science Prac Lesson with Id {id}", request.LessonId);

            return Result.Failure<LessonDetailsResponse>(DomainErrors.SciencePracs.Lesson.NotFound(request.LessonId));
        }

        List<string> offerings = new();

        foreach (SciencePracLessonOffering entry in lesson.Offerings)
        {
            CourseOffering offering = await _offeringRepository.GetById(entry.OfferingId, cancellationToken);

            if (offering is null)
                continue;

            offerings.Add(offering.Name);
        }

        List<LessonDetailsResponse.LessonRollSummary> rollSummaries = new();

        foreach (SciencePracRoll roll in lesson.Rolls)
        {
            School school = await _schoolRepository.GetById(roll.SchoolCode, cancellationToken);

            if (school is null)
                continue;

            LessonDetailsResponse.LessonRollSummary rollSummary = new(
                roll.Id,
                school.Code,
                school.Name,
                roll.Status,
                roll.Attendance.Count(attendance => attendance.Present),
                roll.Attendance.Count(),
                roll.NotificationCount);

            rollSummaries.Add(rollSummary);
        }

        LessonDetailsResponse response = new(
            lesson.Id,
            lesson.Name,
            lesson.DueDate,
            offerings,
            rollSummaries);

        return response;
    }
}

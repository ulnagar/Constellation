﻿namespace Constellation.Application.Domains.SciencePracs.Queries.GetLessonDetails;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.SciencePracs;
using Core.Models.SciencePracs.Errors;
using Core.Models.Subjects;
using Core.Models.Subjects.Identifiers;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetLessonDetailsQueryHandler
    : IQueryHandler<GetLessonDetailsQuery, LessonDetailsResponse>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger _logger;

    public GetLessonDetailsQueryHandler(
        ILessonRepository lessonRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        ISchoolRepository schoolRepository,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _schoolRepository = schoolRepository;
        _logger = logger.ForContext<GetLessonDetailsQuery>();
    }

    public async Task<Result<LessonDetailsResponse>> Handle(GetLessonDetailsQuery request, CancellationToken cancellationToken)
    {
        SciencePracLesson lesson = await _lessonRepository.GetById(request.LessonId, cancellationToken);

        if (lesson is null)
        {
            _logger.Warning("Could not find Science Prac Lesson with Id {id}", request.LessonId);

            return Result.Failure<LessonDetailsResponse>(SciencePracLessonErrors.NotFound(request.LessonId));
        }

        List<string> offerings = new();

        foreach (SciencePracLessonOffering entry in lesson.Offerings)
        {
            Offering offering = await _offeringRepository.GetById(entry.OfferingId, cancellationToken);

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

            bool overdue = lesson.DueDate <= DateOnly.FromDateTime(DateTime.Today) && roll.Status == Core.Enums.LessonStatus.Active;

            LessonDetailsResponse.LessonRollSummary rollSummary = new(
                roll.Id,
                school.Code,
                school.Name,
                roll.Status,
                roll.Attendance.Count(attendance => attendance.Present),
                roll.Attendance.Count(),
                roll.NotificationCount,
                overdue);

            rollSummaries.Add(rollSummary);
        }

        Course course = await _courseRepository.GetByLessonId(lesson.Id, cancellationToken);

        string courseName = $"{course?.Grade} {course?.Name}";

        LessonDetailsResponse response = new(
            lesson.Id,
            course?.Id ?? CourseId.Empty,
            courseName,
            lesson.Name,
            lesson.DueDate,
            offerings,
            rollSummaries);

        return response;
    }
}

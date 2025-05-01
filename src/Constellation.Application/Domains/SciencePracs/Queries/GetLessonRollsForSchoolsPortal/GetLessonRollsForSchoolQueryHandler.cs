namespace Constellation.Application.Domains.SciencePracs.Queries.GetLessonRollsForSchoolsPortal;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Identifiers;
using Core.Models.Offerings.Repositories;
using Core.Models.SciencePracs;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetLessonRollsForSchoolQueryHandler 
    : IQueryHandler<GetLessonRollsForSchoolQuery, List<ScienceLessonRollSummary>>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;

    public GetLessonRollsForSchoolQueryHandler(
        ILessonRepository lessonRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository)
    {
        _lessonRepository = lessonRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
    }

    public async Task<Result<List<ScienceLessonRollSummary>>> Handle(GetLessonRollsForSchoolQuery request, CancellationToken cancellationToken)
    {
        List<ScienceLessonRollSummary> responses = new();

        List<SciencePracLesson> lessons = await _lessonRepository.GetAllForSchool(request.SchoolCode, cancellationToken);

        foreach (SciencePracLesson lesson in lessons)
        {
            SciencePracRoll roll = lesson.Rolls.FirstOrDefault(roll => roll.SchoolCode == request.SchoolCode);

            if (roll is null)
                continue;

            if (roll.Status == Core.Enums.LessonStatus.Cancelled)
                continue;

            OfferingId offeringId = lesson.Offerings.First().OfferingId;

            Offering offering = await _offeringRepository.GetById(offeringId, cancellationToken);

            Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

            int totalStudents = roll.Attendance.Count();
            int presentStudents = roll.Attendance.Count(entry => entry.Present);

            ScienceLessonRollSummary summary = new()
            {
                Id = roll.Id,
                LessonId = lesson.Id,
                LessonName = lesson.Name,
                LessonDueDate = lesson.DueDate.ToDateTime(TimeOnly.MinValue),
                IsSubmitted = roll.Status == Core.Enums.LessonStatus.Completed,
                LessonGrade = course.Grade,
                LessonCourseName = course.Name,
                Statistics = $"{presentStudents}/{totalStudents}"
            };

            responses.Add(summary);
        }

        return responses;
    }
}

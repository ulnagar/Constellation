namespace Constellation.Application.Courses.GetCourseSummary;

using Constellation.Application.Abstractions.Messaging;

public sealed record GetCourseSummaryQuery(
    int CourseId)
    : IQuery<CourseSummaryResponse>;
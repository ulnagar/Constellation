namespace Constellation.Application.Domains.Courses.Queries.GetCourseSummary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.Courses.Models;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record GetCourseSummaryQuery(
    CourseId CourseId)
    : IQuery<CourseSummaryResponse>;
namespace Constellation.Application.Courses.GetCourseDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record GetCourseDetailsQuery(
    CourseId CourseId)
    : IQuery<CourseDetailsResponse>;
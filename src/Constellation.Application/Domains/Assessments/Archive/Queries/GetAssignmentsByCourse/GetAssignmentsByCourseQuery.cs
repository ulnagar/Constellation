namespace Constellation.Application.Domains.Assessments.Archive.Queries.GetAssignmentsByCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;

public sealed record GetAssignmentsByCourseQuery(
    CourseId CourseId,
    StudentId StudentId)
    : IQuery<List<CourseAssignmentResponse>>;
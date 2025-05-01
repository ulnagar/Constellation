namespace Constellation.Application.Domains.Assignments.Queries.GetAssignmentsByCourse;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using Core.Models.Subjects.Identifiers;
using System.Collections.Generic;

public sealed record GetAssignmentsByCourseQuery(
    CourseId CourseId,
    StudentId StudentId)
    : IQuery<List<CourseAssignmentResponse>>;
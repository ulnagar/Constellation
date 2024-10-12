namespace Constellation.Application.Assignments.GetAssignmentsByCourse;

using Abstractions.Messaging;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetAssignmentsByCourseQuery(
    CourseId CourseId,
    StudentId StudentId)
    : IQuery<List<CourseAssignmentResponse>>;
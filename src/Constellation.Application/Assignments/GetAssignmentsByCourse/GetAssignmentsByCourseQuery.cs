namespace Constellation.Application.Assignments.GetAssignmentsByCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;

public sealed record GetAssignmentsByCourseQuery(
    CourseId CourseId,
    string StudentId)
    : IQuery<List<CourseAssignmentResponse>>;
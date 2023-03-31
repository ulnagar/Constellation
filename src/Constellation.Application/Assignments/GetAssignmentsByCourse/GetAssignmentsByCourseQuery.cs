namespace Constellation.Application.Assignments.GetAssignmentsByCourse;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAssignmentsByCourseQuery(
    int CourseId,
    string StudentId)
    : IQuery<List<CourseAssignmentResponse>>;
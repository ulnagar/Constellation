namespace Constellation.Application.Assignments.GetAssignmentsFromCourse;

using Abstractions.Messaging;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;

public sealed record GetAssignmentsFromCourseQuery(
        CourseId CourseId)
    : IQuery<List<AssignmentFromCourseResponse>>;
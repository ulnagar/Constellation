namespace Constellation.Application.Assignments.GetPublishedAssignmentsFromCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Assignments.Models;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;

public sealed record GetPublishedAssignmentsFromCourseQuery(
    CourseId CourseId)
    : IQuery<List<AssignmentFromCourseResponse>>;
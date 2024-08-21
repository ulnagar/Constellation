namespace Constellation.Application.Assignments.GetUploadAssignmentsFromCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Assignments.Models;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;

public sealed record GetUploadAssignmentsFromCourseQuery(
    CourseId CourseId)
    : IQuery<List<AssignmentFromCourseResponse>>;
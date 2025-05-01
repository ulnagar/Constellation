namespace Constellation.Application.Domains.Assignments.Queries.GetUploadAssignmentsFromCourse;

using Abstractions.Messaging;
using Core.Models.Subjects.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetUploadAssignmentsFromCourseQuery(
    CourseId CourseId)
    : IQuery<List<AssignmentFromCourseResponse>>;
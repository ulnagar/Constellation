namespace Constellation.Application.Domains.Assignments.Queries.GetPublishedAssignmentsFromCourse;

using Abstractions.Messaging;
using Core.Models.Subjects.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetPublishedAssignmentsFromCourseQuery(
    CourseId CourseId)
    : IQuery<List<AssignmentFromCourseResponse>>;
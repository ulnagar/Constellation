namespace Constellation.Application.Domains.Assessments.Archive.Queries.GetPublishedAssignmentsFromCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Subjects.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetPublishedAssignmentsFromCourseQuery(
    CourseId CourseId)
    : IQuery<List<AssignmentFromCourseResponse>>;
namespace Constellation.Application.Domains.Assessments.Archive.Queries.GetUploadAssignmentsFromCourse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Subjects.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetUploadAssignmentsFromCourseQuery(
    CourseId CourseId)
    : IQuery<List<AssignmentFromCourseResponse>>;
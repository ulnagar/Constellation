namespace Constellation.Application.Domains.Assessments.Archive.Queries.GetAssignmentsByCourse;

using Core.Models.Assessments.Archive.Identifiers;
using System;

public sealed record CourseAssignmentResponse(
    AssignmentId AssignmentId,
    string Name,
    string DisplayName,
    DateTime DueDate);
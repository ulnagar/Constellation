namespace Constellation.Application.Domains.Assignments.Queries.GetAssignmentsByCourse;

using Core.Models.Assignments.Identifiers;
using System;

public sealed record CourseAssignmentResponse(
    AssignmentId AssignmentId,
    string Name,
    string DisplayName,
    DateTime DueDate);
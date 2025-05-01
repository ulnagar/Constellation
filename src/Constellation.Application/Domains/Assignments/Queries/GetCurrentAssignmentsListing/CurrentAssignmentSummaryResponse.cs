namespace Constellation.Application.Domains.Assignments.Queries.GetCurrentAssignmentsListing;

using Core.Models.Assignments.Identifiers;
using System;

public sealed record CurrentAssignmentSummaryResponse(
    AssignmentId AssignmentId,
    string CourseName,
    string AssignmentName,
    DateOnly DueDate);
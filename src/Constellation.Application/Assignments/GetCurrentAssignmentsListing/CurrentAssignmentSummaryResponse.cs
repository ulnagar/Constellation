namespace Constellation.Application.Assignments.GetCurrentAssignmentsListing;

using Constellation.Core.Models.Assignments.Identifiers;
using System;

public sealed record CurrentAssignmentSummaryResponse(
    AssignmentId AssignmentId,
    string CourseName,
    string AssignmentName,
    DateOnly DueDate);
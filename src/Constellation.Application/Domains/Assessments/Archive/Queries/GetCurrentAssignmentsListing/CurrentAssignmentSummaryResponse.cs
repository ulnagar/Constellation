namespace Constellation.Application.Domains.Assessments.Archive.Queries.GetCurrentAssignmentsListing;

using Core.Models.Assessments.Archive.Identifiers;
using System;

public sealed record CurrentAssignmentSummaryResponse(
    AssignmentId AssignmentId,
    string CourseName,
    string AssignmentName,
    DateOnly DueDate);
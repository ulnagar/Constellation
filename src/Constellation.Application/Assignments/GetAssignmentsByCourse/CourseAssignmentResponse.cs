namespace Constellation.Application.Assignments.GetAssignmentsByCourse;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record CourseAssignmentResponse(
    AssignmentId AssignmentId,
    string Name,
    DateOnly DueDate);
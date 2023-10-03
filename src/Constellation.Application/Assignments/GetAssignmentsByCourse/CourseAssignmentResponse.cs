namespace Constellation.Application.Assignments.GetAssignmentsByCourse;

using Constellation.Core.Models.Assignments.Identifiers;
using System;

public sealed record CourseAssignmentResponse(
    AssignmentId AssignmentId,
    string Name,
    DateTime DueDate);
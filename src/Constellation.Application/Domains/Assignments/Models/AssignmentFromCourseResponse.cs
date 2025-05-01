namespace Constellation.Application.Domains.Assignments.Models;

using Core.Models.Canvas.Models;
using System;

public sealed record AssignmentFromCourseResponse(
    string Name,
    CanvasCourseCode CourseCode,
    int CanvasAssignmentId,
    DateTime DueDate,
    DateTime? LockDate,
    DateTime? UnlockDate,
    int AllowedAttempts,
    bool ExistsInDatabase);
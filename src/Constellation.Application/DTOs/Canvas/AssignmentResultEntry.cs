namespace Constellation.Application.DTOs.Canvas;

using Core.Models.Canvas.Models;
using System.Collections.Generic;

public sealed record AssignmentResultEntry(
    int AssignmentId,
    CanvasCourseCode CourseCode,
    int UserId,
    List<AssignmentResultEntry.AssignmentRubricResult> Marks)
{
    public sealed record AssignmentRubricResult(
        string CriterionId,
        string RatingId,
        string Comments,
        double? Points);
}
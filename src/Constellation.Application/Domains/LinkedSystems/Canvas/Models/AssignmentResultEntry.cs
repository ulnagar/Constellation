namespace Constellation.Application.Domains.LinkedSystems.Canvas.Models;

using Core.Models.Canvas.Models;
using System;
using System.Collections.Generic;

public sealed record AssignmentResultEntry(
    int AssignmentId,
    CanvasCourseCode CourseCode,
    int UserId,
    List<AssignmentResultEntry.AssignmentRubricResult> Marks,
    List<AssignmentResultEntry.AssignmentComment> Comments,
    double? OverallPoints,
    string OverallGrade)
{
    public sealed record AssignmentRubricResult(
        string CriterionId,
        string RatingId,
        string Comments,
        double? Points);

    public sealed record AssignmentComment(
        string Author,
        DateTime CreatedAt,
        string Comment);
}
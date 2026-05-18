namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetCanvasCoursesAndAssessments;

using Core.Models.Canvas.Models;
using System;
using System.Collections.Generic;

public sealed record CanvasCourseWithAssessmentResponse(
    CanvasCourseCode CourseCode,
    string CourseName,
    List<CanvasCourseWithAssessmentResponse.Assessment> Assessments)
{
    public sealed record Assessment(
        int AssessmentId,
        string Name,
        DateTimeOffset? DueDate,
        DateTimeOffset? AvailableFrom,
        DateTimeOffset? AvailableTo,
        int AllowedAttempts);
}

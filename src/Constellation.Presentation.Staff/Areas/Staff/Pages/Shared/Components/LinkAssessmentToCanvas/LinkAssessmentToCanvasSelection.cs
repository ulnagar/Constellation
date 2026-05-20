namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.LinkAssessmentToCanvas;

using Application.Domains.Assessments.Assessments.Queries.GetCanvasCoursesAndAssessments;
using System.Collections.Generic;

public sealed class LinkAssessmentToCanvasSelection(List<CanvasCourseWithAssessmentResponse> courses)
{
    public List<CanvasCourseWithAssessmentResponse> Courses { get; private set; } = courses;
}

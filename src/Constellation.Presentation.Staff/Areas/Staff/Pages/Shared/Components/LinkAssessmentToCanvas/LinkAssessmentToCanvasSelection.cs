namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.LinkAssessmentToCanvas;

using Application.Domains.Assessments.Assessments.Queries.GetCanvasCoursesAndAssessments;
using System.Collections.Generic;

public sealed class LinkAssessmentToCanvasSelection
{
    public List<CanvasCourseWithAssessmentResponse> Courses { get; set; } = [];
}

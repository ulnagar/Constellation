namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.LinkAssessmentToCanvas;

using Constellation.Application.Domains.Assessments.Assessments.Queries.GetCanvasCoursesAndAssessments;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public sealed class LinkAssessmentToCanvasViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public LinkAssessmentToCanvasViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(List<CanvasCourseWithAssessmentResponse> courses)
    {
        List<SelectListItem> selectList = courses
            .SelectMany(course =>
            {
                var group = new SelectListGroup { Name = course.CourseName };

                return course.Assessments.Select(assessment => new SelectListItem
                {
                    Value = $"{course.CourseCode}:{assessment.AssessmentId}",
                    Text = assessment.Name,
                    Group = group
                });
            })
            .ToList();

        LinkAssessmentToCanvasSelection viewModel = new()
        {
            Courses = selectList
        };

        return View(viewModel);
    }
}

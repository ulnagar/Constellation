namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.UploadAssignmentSubmission;

using Constellation.Application.Students.GetStudentsFromCourseAsDictionary;
using Constellation.Core.Models.Subjects.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class UploadAssignmentSubmissionViewComponent : ViewComponent
{
    private readonly IMediator _mediator;

    public UploadAssignmentSubmissionViewComponent(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(CourseId CourseId)
    {
        var viewModel = new AssignmentStudentSelection();
        var result = await _mediator.Send(new GetStudentsFromCourseAsDictionaryQuery(CourseId));

        if (result.IsFailure)
        {
            // How? This method does not return failure
        }
        else
        {
            viewModel.StudentList = result.Value;
        }

        return View(viewModel);
    }
}

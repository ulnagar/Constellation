namespace Constellation.Presentation.Server.Areas.Subject.Pages.Courses;

using Constellation.Application.Courses.GetCourseDetails;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => SubjectPages.Courses;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public CourseDetailsResponse Course { get; set; }

    public async Task OnGet()
    {
        CourseId courseId = CourseId.FromValue(Id);

        Result<CourseDetailsResponse> request = await _mediator.Send(new GetCourseDetailsQuery(courseId));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Courses/Index", values: new { area = "Subjects" })
            };

            return;
        }

        Course = request.Value;
    }
}

namespace Constellation.Presentation.Server.Areas.Subject.Pages.Courses;

using Constellation.Application.Courses.GetCourseSummaryList;
using Constellation.Application.Courses.Models;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => SubjectPages.Courses;

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Active;

    public List<CourseSummaryResponse> Courses { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<CourseSummaryResponse>> request = await _mediator.Send(new GetCourseSummaryListQuery());

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return;
        }

        Courses = Filter switch
        {
            FilterDto.All => request.Value,
            FilterDto.Active => request.Value.Where(course => course.Offerings.Any(offering => offering.IsCurrent)).ToList(),
            FilterDto.Inactive => request.Value.Where(course => course.Offerings.All(offering => !offering.IsCurrent)).ToList(),
            _ => request.Value
        };
    }

    public enum FilterDto
    {
        All,
        Active,
        Inactive
    }
}

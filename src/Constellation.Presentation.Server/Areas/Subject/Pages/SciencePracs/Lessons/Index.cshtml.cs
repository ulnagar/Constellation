namespace Constellation.Presentation.Server.Areas.Subject.Pages.SciencePracs.Lessons;

using Constellation.Application.Models.Auth;
using Constellation.Application.SciencePracs.GetLessonsFromCurrentYear;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;

    public IndexModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [ViewData]
    public string ActivePage => "Lessons";

    public List<LessonSummaryResponse> Lessons { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        Result<List<LessonSummaryResponse>> lessonRequest = await _mediator.Send(new GetLessonsFromCurrentYearQuery());

        if (lessonRequest.IsFailure)
        {
            Error = new()
            {
                Error = lessonRequest.Error,
                RedirectPath = null
            };

            return;
        }

        Lessons = lessonRequest.Value;
    }
}
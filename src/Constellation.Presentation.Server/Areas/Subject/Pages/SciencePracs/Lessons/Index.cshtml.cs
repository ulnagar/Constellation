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

    [ViewData] public string ActivePage => SubjectPages.Lessons;

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Current;

    public List<LessonSummaryResponse> Lessons { get; set; } = new();

    public async Task OnGet()
    {
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

        Lessons = Filter switch
        {
            FilterDto.All => lessonRequest.Value,
            FilterDto.Overdue => lessonRequest.Value.Where(lesson => lesson.Overdue).ToList(),
            FilterDto.Current => lessonRequest.Value.Where(lesson => lesson.Overdue || lesson.DueDate >= DateOnly.FromDateTime(DateTime.Today)).ToList(),
            FilterDto.Future => lessonRequest.Value.Where(lesson => lesson.DueDate >= DateOnly.FromDateTime(DateTime.Today)).ToList(),
            _ => lessonRequest.Value
        };

        Lessons = Lessons.OrderBy(lesson => lesson.DueDate).ToList();
    }

    public enum FilterDto
    {
        All,
        Overdue,
        Current,
        Future
    }
}

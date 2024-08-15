namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Lessons;

using Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.SciencePracs.GetLessonsFromCurrentYear;
using Constellation.Core.Shared;
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Lessons;

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Current;

    public List<LessonSummaryResponse> Lessons { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<LessonSummaryResponse>> lessonRequest = await _mediator.Send(new GetLessonsFromCurrentYearQuery());

        if (lessonRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(lessonRequest.Error);

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

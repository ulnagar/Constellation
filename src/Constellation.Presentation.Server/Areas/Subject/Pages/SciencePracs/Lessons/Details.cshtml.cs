namespace Constellation.Presentation.Server.Areas.Subject.Pages.SciencePracs.Lessons;

using Constellation.Application.Models.Auth;
using Constellation.Application.SciencePracs.CancelLesson;
using Constellation.Application.SciencePracs.GetLessonDetails;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => SubjectPages.Lessons;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public LessonDetailsResponse Lesson { get; set; }

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        SciencePracLessonId lessonId = SciencePracLessonId.FromValue(Id);

        Result<LessonDetailsResponse> lessonRequest = await _mediator.Send(new GetLessonDetailsQuery(lessonId));

        if (lessonRequest.IsFailure)
        {
            Error = new()
            {
                Error = lessonRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SciencePracs/Lessons", values: new { area = "Subject" })
            };

            return;
        }

        Lesson = lessonRequest.Value;
    }

    public async Task<IActionResult> OnGetCancel()
    {
        SciencePracLessonId lessonId = SciencePracLessonId.FromValue(Id);

        Result cancelRequest = await _mediator.Send(new CancelLessonCommand(lessonId));

        if (cancelRequest.IsFailure)
        {
            Error = new()
            {
                Error = cancelRequest.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("Index", new { area = "Subject" });
    }
}

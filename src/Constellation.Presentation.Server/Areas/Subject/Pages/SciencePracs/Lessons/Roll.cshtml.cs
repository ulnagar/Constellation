namespace Constellation.Presentation.Server.Areas.Subject.Pages.SciencePracs.Lessons;

using Constellation.Application.Models.Auth;
using Constellation.Application.SciencePracs.CancelLesson;
using Constellation.Application.SciencePracs.CancelLessonRoll;
using Constellation.Application.SciencePracs.GetLessonRollDetails;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class RollModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public RollModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData]
    public string ActivePage => "Lessons";

    [BindProperty(SupportsGet = true)]
    public Guid LessonId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid RollId { get; set; }

    public LessonRollDetailsResponse Roll { get; set; }

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        SciencePracLessonId sciencePracLessonId = SciencePracLessonId.FromValue(LessonId);
        SciencePracRollId sciencePracRollId = SciencePracRollId.FromValue(RollId);

        Result<LessonRollDetailsResponse> rollRequest = await _mediator.Send(new GetLessonRollDetailsQuery(sciencePracLessonId, sciencePracRollId));
    
        if (rollRequest.IsFailure)
        {
            Error = new()
            {
                Error = rollRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SciencePracs/Lessons/Details", values: new { area = "Subject", id = LessonId })
            };

            return;
        }

        Roll = rollRequest.Value;
    }

    public async Task<IActionResult> OnGetCancel()
    {
        SciencePracLessonId sciencePracLessonId = SciencePracLessonId.FromValue(LessonId);
        SciencePracRollId sciencePracRollId = SciencePracRollId.FromValue(RollId);

        Result cancelRequest = await _mediator.Send(new CancelLessonRollCommand(sciencePracLessonId, sciencePracRollId));

        if (cancelRequest.IsFailure)
        {
            Error = new()
            {
                Error = cancelRequest.Error,
                RedirectPath = null
            };

            Result<LessonRollDetailsResponse> rollRequest = await _mediator.Send(new GetLessonRollDetailsQuery(sciencePracLessonId, sciencePracRollId));

            return Page();
        }

        return RedirectToPage("Index", new { area = "Subject" });
    }

    public async Task<IActionResult> OnGetSendNotification(bool increment)
    {


        return Page();
    }
}

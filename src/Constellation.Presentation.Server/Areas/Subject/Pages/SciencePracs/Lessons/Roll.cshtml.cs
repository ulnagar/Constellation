namespace Constellation.Presentation.Server.Areas.Subject.Pages.SciencePracs.Lessons;

using Constellation.Application.Models.Auth;
using Constellation.Application.SciencePracs.CancelLessonRoll;
using Constellation.Application.SciencePracs.GetLessonRollDetails;
using Constellation.Application.SciencePracs.ReinstateLessonRoll;
using Constellation.Application.SciencePracs.SendLessonNotification;
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

    [ViewData] public string ActivePage => SubjectPages.Lessons;

    [BindProperty(SupportsGet = true)]
    public Guid LessonId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid RollId { get; set; }

    public LessonRollDetailsResponse Roll { get; set; }

    public async Task<IActionResult> OnGet()
    {
        return await PreparePage();
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

            return await PreparePage();
        }

        return RedirectToPage("Index", new { area = "Subject" });
    }

    public async Task<IActionResult> OnGetReinstate()
    {
        SciencePracLessonId sciencePracLessonId = SciencePracLessonId.FromValue(LessonId);
        SciencePracRollId sciencePracRollId = SciencePracRollId.FromValue(RollId);

        Result reinstateRequest = await _mediator.Send(new ReinstateLessonRollCommand(sciencePracLessonId, sciencePracRollId));

        if (reinstateRequest.IsFailure)
        {
            Error = new()
            {
                Error = reinstateRequest.Error,
                RedirectPath = null
            };
        }

        return await PreparePage();
    }

    public async Task<IActionResult> OnGetSendNotification(bool increment)
    {
        SciencePracLessonId sciencePracLessonId = SciencePracLessonId.FromValue(LessonId);
        SciencePracRollId sciencePracRollId = SciencePracRollId.FromValue(RollId);

        Result notificationRequest = await _mediator.Send(new SendLessonNotificationCommand(sciencePracLessonId, sciencePracRollId, increment));

        if (notificationRequest.IsFailure)
        {
            Error = new()
            {
                Error = notificationRequest.Error,
                RedirectPath = null
            };
        }

        return await PreparePage();
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken = default)
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

            return Page();
        }

        Roll = rollRequest.Value;

        return Page();
    }
}

namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Lessons;

using Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.SciencePracs.CancelLessonRoll;
using Constellation.Application.SciencePracs.GetLessonRollDetails;
using Constellation.Application.SciencePracs.ReinstateLessonRoll;
using Constellation.Application.SciencePracs.SendLessonNotification;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class RollModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public RollModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Lessons;

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
            ModalContent = new ErrorDisplay(cancelRequest.Error);

            return await PreparePage();
        }

        return RedirectToPage("/Subject/SciencePracs/Lessons/Index", new { area = "Staff" });
    }

    public async Task<IActionResult> OnGetReinstate()
    {
        SciencePracLessonId sciencePracLessonId = SciencePracLessonId.FromValue(LessonId);
        SciencePracRollId sciencePracRollId = SciencePracRollId.FromValue(RollId);

        Result reinstateRequest = await _mediator.Send(new ReinstateLessonRollCommand(sciencePracLessonId, sciencePracRollId));

        if (reinstateRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(reinstateRequest.Error);
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
            ModalContent = new ErrorDisplay(notificationRequest.Error);
        }

        return await PreparePage();
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken = default)
    {
        SciencePracLessonId sciencePracLessonId = SciencePracLessonId.FromValue(LessonId);
        SciencePracRollId sciencePracRollId = SciencePracRollId.FromValue(RollId);

        Result<LessonRollDetailsResponse> rollRequest = await _mediator.Send(new GetLessonRollDetailsQuery(sciencePracLessonId, sciencePracRollId));

        if (rollRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                rollRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/SciencePracs/Lessons/Details", values: new { area = "Staff", id = LessonId }));

            return Page();
        }

        Roll = rollRequest.Value;

        return Page();
    }
}

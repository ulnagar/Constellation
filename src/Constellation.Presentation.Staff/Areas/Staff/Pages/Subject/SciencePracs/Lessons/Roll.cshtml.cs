namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Lessons;

using Application.Common.PresentationModels;
using Application.Domains.SciencePracs.Commands.CancelLessonRoll;
using Application.Domains.SciencePracs.Commands.ReinstateLessonRoll;
using Application.Domains.SciencePracs.Commands.SendLessonNotification;
using Application.Domains.SciencePracs.Queries.GetLessonRollDetails;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using Shared.Components.CancelRoll;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class RollModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public RollModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<RollModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Lessons;
    [ViewData] public string PageTitle { get; set; } = "Lesson Roll";

    [BindProperty(SupportsGet = true)]
    public SciencePracLessonId LessonId { get; set; }

    [BindProperty(SupportsGet = true)]
    public SciencePracRollId RollId { get; set; }

    public LessonRollDetailsResponse Roll { get; set; }

    public async Task<IActionResult> OnGet()
    {
        return await PreparePage();
    }

    public async Task<IActionResult> OnPostCancel(
        CancelRollSelection viewModel)
    {
        CancelLessonRollCommand command = new(LessonId, RollId, viewModel.Comment);

        _logger
            .ForContext(nameof(CancelLessonRollCommand), command, true)
            .Information("Requested to cancel Lesson Roll by user {User}", _currentUserService);

        Result cancelRequest = await _mediator.Send(command);

        if (cancelRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), cancelRequest.Error, true)
                .Warning("Failed to cancel Lesson Roll by user {User}", _currentUserService);

            ModalContent = ErrorDisplay.Create(cancelRequest.Error);

            return await PreparePage();
        }

        return RedirectToPage("/Subject/SciencePracs/Lessons/Index", new { area = "Staff" });
    }

    public async Task<IActionResult> OnGetReinstate()
    {
        ReinstateLessonRollCommand command = new(LessonId, RollId);

        _logger
            .ForContext(nameof(ReinstateLessonRollCommand), command, true)
            .Information("Requested to reinstate Lesson Roll by user {User}", _currentUserService);

        Result reinstateRequest = await _mediator.Send(command);

        if (reinstateRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), reinstateRequest.Error, true)
                .Warning("Failed to reinstate Lesson Roll by user {User}", _currentUserService);

            ModalContent = ErrorDisplay.Create(reinstateRequest.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetSendNotification(bool increment)
    {
        SendLessonNotificationCommand command = new(LessonId, RollId, increment);

        _logger
            .ForContext(nameof(SendLessonNotificationCommand), command, true)
            .Information("Requested to send notification for Lesson Roll by user {User}", _currentUserService.UserName);

        Result notificationRequest = await _mediator.Send(command);

        if (notificationRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), notificationRequest.Error, true)
                .Warning("Failed to send notification for Lesson Roll by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(notificationRequest.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage();
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken = default)
    {
        _logger.Information("Requested to retrieve details of Lesson Roll with id {Id} by user {User}", RollId, _currentUserService.UserName);
        
        Result<LessonRollDetailsResponse> rollRequest = await _mediator.Send(new GetLessonRollDetailsQuery(LessonId, RollId), cancellationToken);

        if (rollRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), rollRequest.Error, true)
                .Warning("Failed to retrieve details of Lesson Roll with id {Id} by user {User}", RollId, _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                rollRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/SciencePracs/Lessons/Details", values: new { area = "Staff", id = LessonId }));

            return Page();
        }

        Roll = rollRequest.Value;
        PageTitle = $"Details - {Roll.Name}";

        return Page();
    }
}

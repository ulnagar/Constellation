namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Tutorials.Requests;

using Application.Common.PresentationModels;
using Application.Domains.Tutorials.Requests.Commands.ApproveTutorialRequest;
using Application.Domains.Tutorials.Requests.Commands.RejectTutorialRequest;
using Application.Domains.Tutorials.Requests.Queries.GetTutorialRequestById;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Models.Tutorials.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using Shared.Components.ReviewTutorialRequest;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData]
    public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Tutorials_Requests;

    [ViewData]
    public string PageTitle { get; set; } = "Tutorial Request";

    [BindProperty(SupportsGet = true)]
    public RequestId Id { get; set; } = RequestId.Empty;

    public TutorialRequestDetailsResponse Request { get; set; }

    public async Task OnGet()
    {
        Result<TutorialRequestDetailsResponse> request = await _mediator.Send(new GetTutorialRequestByIdQuery(Id));

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve Tutorial Request with Id {id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Tutorials/Requests/Index", values: new { area = "Staff" }));

            return;
        }

        Request = request.Value;
    }

    public async Task<IActionResult> OnPostApprove(
        ReviewTutorialRequestSelection viewModel)
    {
        ApproveTutorialRequestCommand command = new(
            Id,
            viewModel.Comment);

        _logger
            .ForContext(nameof(ApproveTutorialRequestCommand), command, true)
            .Information("Requested to approve Tutorial Request by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(ApproveTutorialRequestCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to approve Tutorial Request by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Subject/Tutorials/Requests/Details", values: new { area = "Staff", Id }));

            return Page();
        }
        
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReject(
        ReviewTutorialRequestSelection viewModel)
    {
        RejectTutorialRequestCommand command = new(
            Id,
            viewModel.Comment);

        _logger
            .ForContext(nameof(RejectTutorialRequestCommand), command, true)
            .Information("Requested to reject Tutorial Request by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(RejectTutorialRequestCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to reject Tutorial Request by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Subject/Tutorials/Requests/Details", values: new { area = "Staff", Id }));

            return Page();
        }

        return RedirectToPage();
    }
}
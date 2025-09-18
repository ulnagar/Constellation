namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Tutorials.Requests;

using Application.Common.PresentationModels;
using Application.Domains.Tutorials.Requests.Queries.GetAllTutorialRequests;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData]
    public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Tutorials_Requests;

    [ViewData]
    public string PageTitle => "Tutorial Requests";

    public List<TutorialRequestSummaryResponse> Requests { get; set; } = [];

    public async Task OnGet()
    {
        Result<List<TutorialRequestSummaryResponse>> tutorialRequests = await _mediator.Send(new GetAllTutorialRequestsQuery());

        if (tutorialRequests.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), tutorialRequests.Error, true)
                .Warning("Failed to retrieve list of Tutorial Requests for user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(tutorialRequests.Error);

            return;
        }

        Requests = tutorialRequests.Value;
    }
}
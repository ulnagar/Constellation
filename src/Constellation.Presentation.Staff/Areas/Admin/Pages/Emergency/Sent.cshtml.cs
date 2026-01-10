namespace Constellation.Presentation.Staff.Areas.Admin.Pages.Emergency;

using Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleMessageEventSummaries;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Abstractions.Services;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

[HasPermission(AuthPermission.Admin_EmergencyConsole_Edit_Value)]
public class SentModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public SentModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [ViewData]
    public string ActivePage => Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Admin_Emergency_Sent;

    [ViewData]
    public string PageTitle => "Emergency - Sent";

    public List<MessageEventSummary> Messages { get; set; } = [];

    public async Task OnGet()
    {
        Result<List<MessageEventSummary>> request = await _mediator.Send(new GetEmergencyConsoleMessageEventSummariesQuery());

        if (request.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(request.Error);

            return;
        }

        Messages = request.Value;
    }
}
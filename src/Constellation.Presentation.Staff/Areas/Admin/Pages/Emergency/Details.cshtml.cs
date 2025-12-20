namespace Constellation.Presentation.Staff.Areas.Admin.Pages.Emergency;

using Application.Common.PresentationModels;
using Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleSentMessageDetails;
using Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Core.Models.EmergencyConsole.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

[Authorize(Policy = AuthPolicies.CanUseEmergencyConsole)]
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
        _logger = logger;
    }

    [ViewData]
    public string ActivePage => Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Admin_Emergency_Sent;

    [ViewData]
    public string PageTitle => "Emergency - Sent";

    [BindProperty(SupportsGet = true)]
    public EventId Id { get; set; } = EventId.Empty;

    public SentMessageDetail Message { get; set; }

    public async Task OnGet()
    {
        Result<SentMessageDetail> message = await _mediator.Send(new GetEmergencyConsoleSentMessageDetailsQuery(Id));

        if (message.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(message.Error);

            return;
        }

        Message = message.Value;
    }
}
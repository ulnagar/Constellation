namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Messaging.Emergency;

using Application.Common.PresentationModels;
using Application.Domains.Messaging.EmergencyConsole.Queries.GetEmergencyConsoleMessageEventDetails;
using Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Core.Models.Messaging.EmergencyConsole.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.Messaging_EmergencyConsole_Edit_Value)]
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
    public string ActivePage => Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Messaging_Emergency_Sent;

    [ViewData]
    public string PageTitle => "Emergency - Sent";

    [BindProperty(SupportsGet = true)]
    public EventId Id { get; set; } = EventId.Empty;

    public MessageEventDetail MessageEvent { get; set; }

    public async Task OnGet()
    {
        Result<MessageEventDetail> message = await _mediator.Send(new GetEmergencyConsoleMessageEventDetailsQuery(Id));

        if (message.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(message.Error);

            return;
        }

        MessageEvent = message.Value;
    }
}
namespace Constellation.Presentation.Staff.Areas.Admin.Pages.Emergency.Templates;

using Application.Common.PresentationModels;
using Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleMessageTemplates;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Models.EmergencyConsole;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

[Authorize(Policy = AuthPolicies.CanUseEmergencyConsole)]
public sealed class IndexModel : BasePageModel
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
            .ForContext<IndexModel>();
    }

    [ViewData]
    public string ActivePage => Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Admin_Emergency_Templates;

    [ViewData]
    public string PageTitle => "Templates";

    public List<MessageTemplate> Templates { get; set; } = [];

    public async Task OnGet()
    {
        Result<List<MessageTemplate>> templates = await _mediator.Send(new GetEmergencyConsoleMessageTemplatesQuery());

        if (templates.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), templates.Error, true)
                .Warning("Failed to retrieve Emergency Console Message Templates for user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(templates.Error);

            return;
        }

        Templates = templates.Value;
    }
}
namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Messaging.History;

using Application.Common.PresentationModels;
using Application.Domains.Messaging.History.Models;
using Application.Domains.Messaging.History.Queries.GetRecentCommunicationsHistory;
using Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.Messaging_View_Value)]
public class RecentModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger _logger;

    public RecentModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _logger = logger
            .ForContext<RecentModel>()
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Messaging_History_List;
    [ViewData] public string PageTitle => "Messaging History";

    public List<CommunicationRecordResponse> Records { get; set; } = [];

    public async Task OnGet()
    {
        var messages = await _mediator.Send(new GetRecentCommunicationsHistoryQuery());

        if (messages.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(messages.Error);

            return;
        }

        Records = messages.Value;
    }
}
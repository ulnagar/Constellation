namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards.Nominations;

using Application.Domains.MeritAwards.Nominations.Queries.GetNotification;
using Application.Models.Auth;
using Constellation.Core.Models.Awards.Identifiers;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.CanViewAwardNominations)]
public class NotificationModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public NotificationModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<NotificationModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Awards_Nominations;
    [ViewData] public string PageTitle { get; set; } = "Award Nomination Notification";

    [BindProperty(SupportsGet = true)]
    public NominationNotificationId Id { get; set; }

    public NotificationResponse Notification { get; set; }

    public async Task OnGet()
    {
        Result<NotificationResponse> notification = await _mediator.Send(new GetNotificationQuery(Id));

        if (notification.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), notification.Error, true)
                .Warning("Failed to retrieve Award Nomination Notification by user {User}", _currentUserService.UserName);

            return;
        }

        Notification = notification.Value;
    }
}
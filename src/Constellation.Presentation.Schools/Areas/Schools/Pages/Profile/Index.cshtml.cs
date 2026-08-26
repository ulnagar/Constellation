namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Profile;

using Application.Domains.Auth.Commands.UpdateUserNotificationPreferences;
using Application.Domains.Auth.Models;
using Application.Domains.Auth.Queries.GetParentUserDetails;
using Application.Domains.Auth.Queries.GetSchoolContactUserDetails;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Extensions;
using Core.Abstractions.Services;
using Core.Models.Auth.Enums;
using Core.Models.Awards.Events;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.SchoolsPortal_View_Value)]
public class IndexModel : BasePageModel
{
    private ISender _mediator => Mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Profile;

    [BindProperty]
    public List<NotificationSetting> Notifications { get; set; } = [];

    public ContactUserResponse CurrentUser { get; set; }

    public List<NotificationType> NotificationTypes => NotificationType.GetEnumerable.ToList();

    public async Task OnGet()
    {
        await PreparePage();
    }

    public async Task<IActionResult> OnPostUpdateNotifications()
    {
        List<NotificationType> enabledNotifications = Notifications.Any()
            ? Notifications
                .Where(entry => entry.Enabled)
                .Select(entry => entry.Type)
                .ToList()
            : [];

        UpdateUserNotificationPreferencesCommand command = new(User.GetUserId(), enabledNotifications);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage();
    }

    public bool IsEnabled(NotificationType type) =>
        CurrentUser.OptedInNotificationTypes.Any(n => n == type);

    private async Task PreparePage()
    {
        Result<ContactUserResponse> user = await _mediator.Send(new GetSchoolContactUserDetailsQuery(User.GetUserId()));

        if (user.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(user.Error);

            return;
        }

        CurrentUser = user.Value;

        foreach (var notification in CurrentUser.OptedInNotificationTypes)
        {
            Notifications.Add(new()
            {
                Type = notification,
                Enabled = true
            });
        }
    }
}
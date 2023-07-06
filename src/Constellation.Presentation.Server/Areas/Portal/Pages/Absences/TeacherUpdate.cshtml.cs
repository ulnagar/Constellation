namespace Constellation.Presentation.Server.Areas.Portal.Pages.Absences;

using Constellation.Application.MissedWork.CreateClassworkNotificationResponse;
using Constellation.Application.MissedWork.GetNotificationDetails;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

[Roles(AuthRoles.Admin, AuthRoles.StaffMember)]
public class TeacherUpdateModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public TeacherUpdateModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public NotificationDetails Notification { get; set; }

    [BindProperty]
    [Required]
    public string Description { get; set; }

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        await PreparePage(cancellationToken);
    }

    private async Task PreparePage(CancellationToken cancellationToken = default)
    {
        await GetClasses(_mediator);

        ClassworkNotificationId notificationId = ClassworkNotificationId.FromValue(Id);

        Result<NotificationDetails> notificationRequest = await _mediator.Send(new GetNotificationDetailsQuery(notificationId), cancellationToken);

        if (notificationRequest.IsFailure)
        {
            Error = new()
            {
                Error = notificationRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Teachers", values: new { area = "Portal" })
            };

            return;
        }

        Notification = notificationRequest.Value;
    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            await PreparePage(cancellationToken);

            return Page();
        }

        ClassworkNotificationId notificationId = ClassworkNotificationId.FromValue(Id);
        string? user = User.Identity.Name;

        Result result = await _mediator.Send(new CreateClassworkNotificationResponseCommand(
            notificationId,
            user,
            Description),
            cancellationToken);

        return RedirectToPage("Teachers", new { area = "Portal"});
    }
}

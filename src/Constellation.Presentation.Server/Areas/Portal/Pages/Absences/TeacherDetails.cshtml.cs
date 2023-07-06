namespace Constellation.Presentation.Server.Areas.Portal.Pages.Absences;

using Constellation.Application.MissedWork.GetNotificationDetails;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

public class TeacherDetailsModel : BasePageModel
{
    private readonly IMediator _mediator;

    public TeacherDetailsModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public NotificationDetails Notification { get; set; }

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        await GetClasses(_mediator);

        ClassworkNotificationId notificationId = ClassworkNotificationId.FromValue(Id);

        Result<NotificationDetails> entry = await _mediator.Send(new GetNotificationDetailsQuery(notificationId), cancellationToken);

        if (entry.IsFailure)
        {
            Error = new()
            {
                Error = entry.Error,
                RedirectPath = null
            };

            return;
        }

        Notification = entry.Value;
    }
}

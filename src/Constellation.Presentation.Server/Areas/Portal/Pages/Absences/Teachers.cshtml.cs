namespace Constellation.Presentation.Server.Areas.Portal.Pages.Absences;

using Constellation.Application.MissedWork.GetNotificationsForTeacher;
using Constellation.Application.MissedWork.Models;
using Constellation.Application.StaffMembers.GetStaffByEmail;
using Constellation.Application.StaffMembers.Models;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class TeachersModel : BasePageModel
{
    private readonly IMediator _mediator;

    public TeachersModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public List<NotificationSummary> Notifications { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; }

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        await GetClasses(_mediator);

        string? username = User.Identity.Name;
        Result<StaffSelectionListResponse> teacherRequest = await _mediator.Send(new GetStaffByEmailQuery(username), cancellationToken);

        if (teacherRequest.IsFailure)
            return;

        Result<List<NotificationSummary>> notificationRequest = await _mediator.Send(new GetNotificationsForTeacherQuery(teacherRequest.Value.StaffId), cancellationToken);

        if (notificationRequest.IsFailure)
            return;

        Notifications = Filter switch
        {
            FilterDto.Complete => notificationRequest.Value.Where(notification => notification.IsCompleted).ToList(),
            FilterDto.All => notificationRequest.Value.ToList(),
            _ => notificationRequest.Value.Where(notification => !notification.IsCompleted).ToList()
        };
    }

    public enum FilterDto
    {
        Pending,
        Complete,
        All
    }
}

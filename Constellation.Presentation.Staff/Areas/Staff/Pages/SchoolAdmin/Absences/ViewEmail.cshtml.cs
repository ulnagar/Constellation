namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Absences;

using Constellation.Application.Absences.GetAbsenceNotificationDetails;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class ViewEmailModel : PageModel
{
    private readonly IMediator _mediator;

    public ViewEmailModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid NotificationGuid { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid AbsenceGuid { get; set; }

    public string Source { get; set; }

    public async Task OnGet()
    {
        AbsenceId absenceId = AbsenceId.FromValue(AbsenceGuid);
        AbsenceNotificationId notificationId = AbsenceNotificationId.FromValue(NotificationGuid);

        Result<string> request = await _mediator.Send(new GetAbsenceNotificationDetailsQuery(absenceId, notificationId));
        if (request.IsFailure)
        {
            Source = $"An error has occurred: {request.Error.Message}";
        }

        Source = request.Value;
    }
}

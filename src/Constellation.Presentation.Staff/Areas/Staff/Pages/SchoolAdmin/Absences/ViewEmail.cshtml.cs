namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Absences;

using Constellation.Application.Absences.GetAbsenceNotificationDetails;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class ViewEmailModel : PageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ViewEmailModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ViewEmailModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(ConstructorBinder))]
    public AbsenceNotificationId NotificationId { get; set; }

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(ConstructorBinder))]
    public AbsenceId AbsenceId { get; set; }

    public string Source { get; set; }

    public async Task OnGet()
    {
        GetAbsenceNotificationDetailsQuery command = new(AbsenceId, NotificationId);

        _logger
            .ForContext(nameof(GetAbsenceNotificationDetailsQuery), command, true)
            .Information("Requested to retrieve Absence Notification email by user {User}", _currentUserService.UserName);

        Result<string> request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve Absence Notification email by user {User}", _currentUserService.UserName);

            Source = $"An error has occurred: {request.Error.Message}";
        }

        Source = request.Value;
    }
}

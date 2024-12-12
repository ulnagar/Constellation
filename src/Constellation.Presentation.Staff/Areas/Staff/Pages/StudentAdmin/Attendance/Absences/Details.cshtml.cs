namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Attendance.Absences;

using Constellation.Application.Absences.GetAbsenceDetails;
using Constellation.Application.Absences.SendAbsenceNotificationToParent;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Constellation.Presentation.Staff.Areas.Staff.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IAuthorizationService authService,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _authService = authService;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Attendance_Absences;
    [ViewData] public string PageTitle { get; set; } = "Absence Details";

    [BindProperty(SupportsGet = true)]
    public AbsenceId Id { get; set; }

    public AbsenceDetailsResponse Absence;

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        _logger.Information("Requested to retrieve details of Absence with id {Id} for user {User}", Id, _currentUserService.UserName);

        Result<AbsenceDetailsResponse> result = await _mediator.Send(new GetAbsenceDetailsQuery(Id), cancellationToken);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed tor retrieve details of Absence with id {Id} for user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                result.Error,
                Request.Headers["Referer"].ToString());

            return;
        }

        Absence = result.Value;
        PageTitle = $"Absence Details - {result.Value.StudentName.DisplayName}";
    }

    public async Task<IActionResult> OnGetSendNotification(
        StudentId studentId,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Requested to send notification for Absence with id {Id} by user {User}", Id, _currentUserService.UserName);

        AuthorizationResult isAuthorised = await _authService.AuthorizeAsync(User, AuthPolicies.CanManageAbsences);

        if (!isAuthorised.Succeeded)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Auth.NotAuthorised, true)
                .Warning("Failed to send notification for Absence with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(DomainErrors.Auth.NotAuthorised);

            return RedirectToPage();
        }

        await _mediator.Send(new SendAbsenceNotificationToParentCommand(Guid.NewGuid(), studentId, new List<AbsenceId> { Id }), cancellationToken);

        return RedirectToPage();
    }
}

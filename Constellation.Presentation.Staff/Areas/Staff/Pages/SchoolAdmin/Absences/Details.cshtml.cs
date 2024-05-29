namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Absences;

using Constellation.Application.Absences.GetAbsenceDetails;
using Constellation.Application.Absences.SendAbsenceNotificationToParent;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authService;

    public DetailsModel(
        IMediator mediator,
        LinkGenerator linkGenerator,
        IAuthorizationService authService)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _authService = authService;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Absences_List;
    
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public AbsenceDetailsResponse Absence;

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        AbsenceId absenceId = AbsenceId.FromValue(Id);

        Result<AbsenceDetailsResponse> result = await _mediator.Send(new GetAbsenceDetailsQuery(absenceId), cancellationToken);

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = Request.Headers["Referer"].ToString()
            };

            return;
        }

        Absence = result.Value;
    }

    public async Task<IActionResult> OnGetSendNotification(string studentId, CancellationToken cancellationToken = default)
    {
        AuthorizationResult isAuthorised = await _authService.AuthorizeAsync(User, AuthPolicies.CanManageAbsences);

        if (isAuthorised.Succeeded)
        {
            AbsenceId absenceId = AbsenceId.FromValue(Id);

            await _mediator.Send(new SendAbsenceNotificationToParentCommand(Guid.NewGuid(), studentId, new List<AbsenceId> { absenceId }), cancellationToken);
        }

        return RedirectToPage();
    }
}

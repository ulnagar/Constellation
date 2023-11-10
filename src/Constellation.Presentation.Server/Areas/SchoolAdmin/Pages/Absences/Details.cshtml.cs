namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Absences;

using Application.Models.Auth;
using Constellation.Application.Absences.GetAbsenceDetails;
using Constellation.Application.Absences.SendAbsenceNotificationToParent;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public AbsenceDetailsResponse Absence;

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        ViewData["ActivePage"] = "Report";

        await GetClasses(_mediator);

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

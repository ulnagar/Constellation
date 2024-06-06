namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Periods;

using Application.Models.Auth;
using Application.Periods.GetPeriodsForVisualSelection;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Subject_Periods_Periods;

    public List<PeriodVisualSelectResponse> Periods { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<PeriodVisualSelectResponse>> request = await _mediator.Send(new GetPeriodsForVisualSelectionQuery());

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return;
        }

        Periods = request.Value;
    }
}
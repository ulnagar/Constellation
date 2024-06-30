namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards.Nominations;

using Constellation.Application.Awards.GetAllNominationPeriods;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.CanViewAwardNominations)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;

    public IndexModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Awards_Nominations;

    public List<NominationPeriodResponse> Periods { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        Result<List<NominationPeriodResponse>> request = await _mediator.Send(new GetAllNominationPeriodsQuery(), cancellationToken);

        if (request.IsSuccess)
        {
            Periods = request.Value;
        }
    }
}

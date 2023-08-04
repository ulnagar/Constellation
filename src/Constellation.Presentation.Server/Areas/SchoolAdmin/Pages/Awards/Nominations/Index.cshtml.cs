namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Awards.Nominations;

using Constellation.Application.Awards.GetAllNominationPeriods;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Core.Models.Awards;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize(Policy = AuthPolicies.CanViewAwardNominations)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;

    public IndexModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [ViewData]
    public string ActivePage => "Nominations";

    public List<NominationPeriodResponse> Periods { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        await GetClasses(_mediator);

        Result<List<NominationPeriodResponse>> request = await _mediator.Send(new GetAllNominationPeriodsQuery(), cancellationToken);

        if (request.IsSuccess)
        {
            Periods = request.Value;
        }
    }
}

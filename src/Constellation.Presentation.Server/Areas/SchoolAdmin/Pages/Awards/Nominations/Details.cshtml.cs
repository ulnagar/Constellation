namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Awards.Nominations;

using Constellation.Application.Awards.GetNominationPeriod;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData]
    public string ActivePage => "Nominations";

    [BindProperty(SupportsGet = true)]
    public Guid PeriodId { get; set; }

    public NominationPeriodDetailResponse Period { get; set; }

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        await GetClasses(_mediator);

        AwardNominationPeriodId awardNominationPeriodId = AwardNominationPeriodId.FromValue(PeriodId);

        Result<NominationPeriodDetailResponse> periodRequest = await _mediator.Send(new GetNominationPeriodRequest(awardNominationPeriodId), cancellationToken);

        if (periodRequest.IsFailure)
        {
            Error = new()
            {
                Error = periodRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Awards/Nominations/Index", values: new { area = "SchoolAdmin" })
            };

            return;
        }

        Period = periodRequest.Value;
    }
}

namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards.Nominations;

using Constellation.Application.Awards.DeleteAwardNomination;
using Constellation.Application.Awards.ExportAwardNominations;
using Constellation.Application.Awards.GetNominationPeriod;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanViewAwardNominations)]
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

    [ViewData] public string ActivePage => "Nominations";

    [BindProperty(SupportsGet = true)]
    public Guid PeriodId { get; set; }

    public NominationPeriodDetailResponse Period { get; set; }

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        AwardNominationPeriodId awardNominationPeriodId = AwardNominationPeriodId.FromValue(PeriodId);

        Result<NominationPeriodDetailResponse> periodRequest = await _mediator.Send(new GetNominationPeriodRequest(awardNominationPeriodId), cancellationToken);

        if (periodRequest.IsFailure)
        {
            Error = new()
            {
                Error = periodRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Index", values: new { area = "Staff" })
            };

            return;
        }

        Period = periodRequest.Value;
    }

    public async Task<IActionResult> OnGetExport(CancellationToken cancellationToken = default)
    {
        AwardNominationPeriodId awardNominationPeriodId = AwardNominationPeriodId.FromValue(PeriodId);

        Result<FileDto> fileRequest = await _mediator.Send(new ExportAwardNominationsCommand(awardNominationPeriodId), cancellationToken);

        if (fileRequest.IsFailure)
        {
            Result<NominationPeriodDetailResponse> periodRequest = await _mediator.Send(new GetNominationPeriodRequest(awardNominationPeriodId), cancellationToken);

            if (periodRequest.IsFailure)
            {
                Error = new()
                {
                    Error = periodRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Index", values: new { area = "Staff" })
                };

                return Page();
            }

            Period = periodRequest.Value;

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }

    public async Task OnGetDelete(Guid entryId, CancellationToken cancellationToken = default)
    {
        AwardNominationPeriodId awardNominationPeriodId = AwardNominationPeriodId.FromValue(PeriodId);
        AwardNominationId awardNominationId = AwardNominationId.FromValue(entryId);

        Result request = await _mediator.Send(new DeleteAwardNominationCommand(awardNominationPeriodId, awardNominationId), cancellationToken);

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };
        }

        Result<NominationPeriodDetailResponse> periodRequest = await _mediator.Send(new GetNominationPeriodRequest(awardNominationPeriodId), cancellationToken);

        if (periodRequest.IsFailure)
        {
            Error = new()
            {
                Error = periodRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Index", values: new { area = "Staff" })
            };

            return;
        }

        Period = periodRequest.Value;

        return;
    }
}

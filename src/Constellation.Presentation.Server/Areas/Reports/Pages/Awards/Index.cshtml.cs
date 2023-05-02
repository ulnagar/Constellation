namespace Constellation.Presentation.Server.Areas.Reports.Pages.Awards;

using Constellation.Application.Awards.GetAllAwards;
using Constellation.Application.Awards.GetAwardCertificate;
using Constellation.Application.Awards.GetRecentAwards;
using Constellation.Application.Awards.Models;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    public enum FilterDto
    {
        All,
        Recent,
        ThisYear
    }

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Recent;

    public List<AwardResponse> Awards = new();

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        await GetClasses(_mediator);

        Result<List<AwardResponse>> awardRequest = Filter switch
        {
            FilterDto.All => await _mediator.Send(new GetAllAwardsQuery(false), cancellationToken),
            FilterDto.Recent => await _mediator.Send(new GetRecentAwardsQuery(20), cancellationToken),
            FilterDto.ThisYear => await _mediator.Send(new GetAllAwardsQuery(true), cancellationToken),
            _ => Result.Failure<List<AwardResponse>>(Core.Shared.Error.NullValue)
        };

        if (awardRequest.IsFailure)
        {
            Error = new()
            {
                Error = awardRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Home" })
            };

            return;
        }

        Awards = awardRequest.Value;

        return;
    }

    public async Task<IActionResult> OnGetAttemptDownload(string Id, CancellationToken cancellationToken = default)
    {
        var awardId = StudentAwardId.FromValue(Guid.Parse(Id));

        var fileRequest = await _mediator.Send(new GetAwardCertificateQuery(awardId), cancellationToken);

        if (fileRequest.IsFailure)
        {
            Error = new()
            {
                Error = fileRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Awards/Index", values: new { area = "Reports" })
            };

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.Name);
    }
}

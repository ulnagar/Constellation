namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Workflows;

using Application.Models.Auth;
using Application.WorkFlows.GetCaseSummaryList;
using BaseModels;
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

    [ViewData] public string ActivePage => WorkFlowPages.Cases;

    public List<CaseSummaryResponse> Cases { get; set; }

    public async Task OnGet()
    {
        Result<List<CaseSummaryResponse>> request = await _mediator.Send(new GetCaseSummaryListQuery());

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return;
        }

        Cases = request.Value;
    }
}
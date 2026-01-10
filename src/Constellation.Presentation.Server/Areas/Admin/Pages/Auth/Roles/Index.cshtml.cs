namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth.Roles;

using Application.Domains.Auth.Queries.GetAuthRolesAsSummary;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Auth_Roles;
    [ViewData] public string PageTitle => "Roles";

    public List<RoleSummaryResponse> Roles { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<RoleSummaryResponse>> roles = await _mediator.Send(new GetAuthRolesAsSummaryQuery());

        if (roles.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(roles.Error);

            return;
        }

        Roles = roles.Value;
    }
}

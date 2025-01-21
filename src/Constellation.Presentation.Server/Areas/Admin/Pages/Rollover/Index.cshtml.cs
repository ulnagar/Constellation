namespace Constellation.Presentation.Server.Areas.Admin.Pages.Rollover;

using Application.Models.Auth;
using BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Rollover;
    [ViewData] public string PageTitle => "Annual Rollover";

    public async Task OnGet() { }
}
namespace Constellation.Presentation.Server.Areas.Equipment.Pages.Assets;

using Application.Models.Auth;
using BaseModels;
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

    [ViewData] public string ActivePage => AssetsPages.Assets;

    public async Task OnGet()
    {
    }
}
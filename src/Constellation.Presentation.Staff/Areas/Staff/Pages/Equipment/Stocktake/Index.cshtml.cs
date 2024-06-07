namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Stocktake;

using Application.Models.Auth;
using Application.Stocktake.GetStocktakeEventList;
using Application.Stocktake.Models;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.CanManageAssets)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Stocktake_List;

    public List<StocktakeEventResponse> Events { get; set; }

    public async Task OnGet()
    {
        Result<List<StocktakeEventResponse>> events = await _mediator.Send(new GetStocktakeEventListQuery());

        if (events.IsFailure)
        {
            Error = new()
            {
                Error = events.Error,
                RedirectPath = null
            };

            return;
        }

        Events = events.Value;
    }
}
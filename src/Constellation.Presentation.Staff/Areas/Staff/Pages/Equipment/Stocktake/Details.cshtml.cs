namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Stocktake;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Stocktake.GetStocktakeEventDetails;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanManageAssets)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Stocktake_List;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public StocktakeEventDetailsResponse Stocktake { get; set; }

    public async Task OnGet()
    {
        Result<StocktakeEventDetailsResponse> stocktake = await _mediator.Send(new GetStocktakeEventDetailsQuery(Id));

        if (stocktake.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                stocktake.Error,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Index", values: new { area = "Staff" }));

            return;
        }

        Stocktake = stocktake.Value;
    }
}

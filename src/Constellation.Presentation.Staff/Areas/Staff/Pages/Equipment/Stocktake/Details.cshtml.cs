namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Stocktake;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Stocktake.GetStocktakeEventDetails;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanManageAssets)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Stocktake_List;
    [ViewData] public string PageTitle => "Stocktake Event Details";

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public StocktakeEventDetailsResponse Stocktake { get; set; }

    public async Task OnGet()
    {
        _logger
            .Information("Requested to retrieve Stocktake Event details with Id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<StocktakeEventDetailsResponse> stocktake = await _mediator.Send(new GetStocktakeEventDetailsQuery(Id));

        if (stocktake.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                stocktake.Error,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), stocktake.Error, true)
                .Warning("Failed to retrieve Stocktake Event details with Id {Id} by user {User}", Id, _currentUserService.UserName);

            return;
        }

        Stocktake = stocktake.Value;
    }
}

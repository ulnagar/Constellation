namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Stocktake;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Stocktake.GetStocktakeEventList;
using Application.Stocktake.Models;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IAuthorizationService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        IAuthorizationService authService,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _authService = authService;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Stocktake_List;
    [ViewData] public string PageTitle => "Stocktake Events";

    public List<StocktakeEventResponse> Events { get; set; }

    public async Task<IActionResult> OnGet()
    {
        _logger
            .Information("Requested to retrieve list of Stocktake Events by user {User}", _currentUserService.UserName);

        Result<List<StocktakeEventResponse>> events = await _mediator.Send(new GetStocktakeEventListQuery());

        if (events.IsFailure)
        {
            ModalContent = new ErrorDisplay(events.Error);

            _logger
                .ForContext(nameof(Error), events.Error, true)
                .Warning("Failed to retrieve list of Stocktake Events by user {User}", _currentUserService.UserName);

            return Page();
        }

        StocktakeEventResponse? currentEvent = events.Value.FirstOrDefault(entry => entry.EndDate >= DateTime.Now);

        if (!(await _authService.AuthorizeAsync(User, AuthPolicies.CanManageAssets)).Succeeded)
        {
            return currentEvent is not null 
                ? RedirectToPage("/Equipment/Stocktake/Dashboard", new { area = "Staff", Id = currentEvent.Id }) 
                : RedirectToPage("/AccessDenied", new { area = "Admin", returnUrl = "/Equipment/Stocktake" });
        }
        
        Events = events.Value;

        return Page();
    }
}
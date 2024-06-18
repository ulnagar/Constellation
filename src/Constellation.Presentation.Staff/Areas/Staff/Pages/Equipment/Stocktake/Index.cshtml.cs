namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Stocktake;

using Application.Models.Auth;
using Application.Stocktake.GetStocktakeEventList;
using Application.Stocktake.Models;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IAuthorizationService _authService;

    public IndexModel(
        ISender mediator,
        IAuthorizationService authService)
    {
        _mediator = mediator;
        _authService = authService;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Stocktake_List;

    public List<StocktakeEventResponse> Events { get; set; }

    public async Task<IActionResult> OnGet()
    {
        Result<List<StocktakeEventResponse>> events = await _mediator.Send(new GetStocktakeEventListQuery());

        if (events.IsFailure)
        {
            Error = new()
            {
                Error = events.Error,
                RedirectPath = null
            };

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
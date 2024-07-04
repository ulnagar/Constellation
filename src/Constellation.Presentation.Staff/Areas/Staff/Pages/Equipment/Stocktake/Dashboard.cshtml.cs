namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Stocktake;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Stocktake.GetStocktakeEvent;
using Application.Stocktake.GetStocktakeSightingsForStaffMember;
using Application.Stocktake.Models;
using Core.Errors;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DashboardModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DashboardModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Stocktake_Dashboard;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public string Name { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public List<StocktakeSightingResponse> Sightings { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (Id == Guid.Empty)
        {
            return RedirectToPage("/Dashboard", new { area = "Staff" });
        }

        string staffId = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(staffId))
        {
            ModalContent = new ErrorDisplay(
                DomainErrors.Auth.UserNotFound,
                _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" }));

            return Page();
        }

        Result<StocktakeEventResponse> eventRequest = await _mediator.Send(new GetStocktakeEventQuery(Id));

        if (eventRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                eventRequest.Error,
                _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" }));

            return Page();
        }

        Result<List<StocktakeSightingResponse>> request = await _mediator.Send(new GetStocktakeSightingsForStaffMemberQuery(staffId, Id));

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Dashboard", values: new { area = "Staff" }));

            return Page();
        }

        Sightings = request.Value;
        Name = eventRequest.Value.Name;
        StartDate = DateOnly.FromDateTime(eventRequest.Value.StartDate);
        EndDate = DateOnly.FromDateTime(eventRequest.Value.EndDate);

        return Page();
    }
}
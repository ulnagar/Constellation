namespace Constellation.Presentation.Shared.ViewComponents;

using Application.Models.Auth;
using Constellation.Application.StaffMembers.GetStaffForSelectionList;
using Constellation.Application.StaffMembers.Models;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pages.Shared.Components.AddAssetSighting;
using System.Security.Claims;

public class AddAssetSightingViewComponent : ViewComponent
{
    private readonly ISender _mediator;
    
    public AddAssetSightingViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        AddAssetSightingSelection viewModel = new();

        Result<List<StaffSelectionListResponse>> staff = await _mediator.Send(new GetStaffForSelectionListQuery());

        if (staff.IsFailure)
        {
            return Content(string.Empty);
        }

        string? currentStaffId = (User as ClaimsPrincipal)?.Claims.FirstOrDefault(entry => entry.Type == AuthClaimType.StaffEmployeeId)?.Value;

        if (currentStaffId is null)
        {
            viewModel.StaffList = new SelectList(staff.Value.OrderBy(entry => entry.LastName), "StaffId", "DisplayName");
        }
        else
        {
            StaffSelectionListResponse? currentStaffMember = staff.Value.FirstOrDefault(entry => entry.StaffId == currentStaffId);

            viewModel.StaffList = new SelectList(
                staff.Value.OrderBy(entry => entry.LastName),
                "StaffId",
                "DisplayName", 
                currentStaffMember?.StaffId);
        }

        DateTime currentDateTime = DateTime.Now;
        currentDateTime = currentDateTime
            .AddMilliseconds(-currentDateTime.Millisecond)
            .AddSeconds(-currentDateTime.Second);

        viewModel.SightedAt = currentDateTime;

        return View(viewModel);
    }
}
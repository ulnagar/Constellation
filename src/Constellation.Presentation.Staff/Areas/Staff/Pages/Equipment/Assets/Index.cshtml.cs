namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Assets;

using Application.Common.PresentationModels;
using Constellation.Application.Assets.Enums;
using Constellation.Application.Assets.ExportAssetsToExcel;
using Constellation.Application.Assets.GetAllActiveAssets;
using Constellation.Application.Assets.GetAllAssets;
using Constellation.Application.Assets.GetAllDisposedAssets;
using Constellation.Application.Assets.Models;
using Constellation.Application.Assets.SightAsset;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Application.StaffMembers.GetStaffForSelectionList;
using Constellation.Application.StaffMembers.Models;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Views.Shared.PartialViews.AddAssetSighting;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }
    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Assets_Assets;
    [ViewData] public string PageTitle => "Assets List";

    [BindProperty(SupportsGet = true)] 
    public AssetFilter Filter { get; set; } = AssetFilter.Active;
    
    public IReadOnlyList<AssetListItem> Assets { get; set; }

    public async Task OnGet()
    {
        Result<List<AssetListItem>> request = Filter switch
        {
            AssetFilter.All => await _mediator.Send(new GetAllAssetsQuery()),
            AssetFilter.Disposed => await _mediator.Send(new GetAllDisposedAssetsQuery()),
            _ => await _mediator.Send(new GetAllActiveAssetsQuery())
        };

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(request.Error);

            return;
        }

        Assets = request.Value;
    }

    public async Task<IActionResult> OnGetExport()
    {
        Result<FileDto> file = await _mediator.Send(new ExportAssetsToExcelQuery(Filter));

        if (file.IsFailure)
        {
            ModalContent = new ErrorDisplay(file.Error);

            return Page();
        }

        return File(file.Value.FileData, file.Value.FileType, file.Value.FileName);
    }

    public async Task<IActionResult> OnPostAjaxAddSighting(string assetNumber)
    {
        AddAssetSightingViewModel viewModel = new();

        viewModel.AssetNumber = AssetNumber.FromValue(assetNumber);

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

            viewModel.StaffList = new(
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

        return Partial("AddAssetSighting", viewModel);
    }

    public async Task<IActionResult> OnPostAddSighting(AddAssetSightingViewModel viewModel)
    {
        SightAssetCommand command = new(
            viewModel.AssetNumber,
            viewModel.StaffId,
            viewModel.SightedAt,
            viewModel.Note ?? string.Empty);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(result.Error);

            return Page();
        }

        return RedirectToPage();
    }
}
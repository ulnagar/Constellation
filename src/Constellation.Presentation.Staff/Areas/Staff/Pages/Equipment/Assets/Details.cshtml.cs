#nullable enable
namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Assets;

using Application.Assets.AddAssetNote;
using Application.Assets.AllocateAsset;
using Application.Assets.DeallocateAsset;
using Application.Assets.SightAsset;
using Application.Assets.TransferAsset;
using Application.Assets.UpdateAssetStatus;
using Constellation.Application.Assets.GetAssetByAssetNumber;
using Constellation.Application.Models.Auth;
using Constellation.Application.StaffMembers.GetStaffForSelectionList;
using Constellation.Application.StaffMembers.Models;
using Constellation.Core.Models.Assets.Errors;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Pages.Shared.PartialViews.AddAssetSighting;
using Constellation.Presentation.Staff.Areas;
using Core.Models.Assets.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.ModelBinders;
using Presentation.Shared.Pages.Shared.Components.AddAssetNote;
using Presentation.Shared.Pages.Shared.Components.AllocateAsset;
using Presentation.Shared.Pages.Shared.Components.TransferAsset;
using Presentation.Shared.Pages.Shared.Components.UpdateAssetStatus;
using System.Security.Claims;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
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

    [ViewData] public string ActivePage => Constellation.Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Assets_Assets;
    [ViewData] public string PageTitle => Asset is null ? "Asset Details" : $"Details - {Asset.AssetNumber}";

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(AssetNumberBinder))]
    public AssetNumber AssetNumber { get; set; }

    public AssetResponse? Asset { get; set; }

    public async Task OnGet()
    {
        if (string.IsNullOrWhiteSpace(AssetNumber))
        {
            Error = new()
            {
                Error = AssetNumberErrors.Empty,
                RedirectPath = _linkGenerator.GetPathByPage("/Equipment/Assets/Index", values: new { area = "Staff" })
            };

            return;
        }

        Result<AssetResponse> result = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Equipment/Assets/Index", values: new { area = "Staff" })
            };

            return;
        }

        Asset = result.Value;
    }
    
    public async Task<IActionResult> OnPostAllocateDevice(AllocateDeviceSelection viewModel)
    {
        if (viewModel.AllocationType.Equals(AllocationType.Student))
        {
            if (string.IsNullOrWhiteSpace(viewModel.StudentId))
            {
                Error = new()
                {
                    Error = AllocationErrors.StudentEmpty,
                    RedirectPath = null
                };

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            AllocateAssetToStudentCommand command = new(
                AssetNumber,
                viewModel.StudentId);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                Error = new()
                {
                    Error = result.Error,
                    RedirectPath = null
                };

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            return RedirectToPage();
        }

        if (viewModel.AllocationType.Equals(AllocationType.Staff))
        {
            if (string.IsNullOrWhiteSpace(viewModel.StaffId))
            {
                Error = new()
                {
                    Error = AllocationErrors.StaffEmpty,
                    RedirectPath = null
                };

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            AllocateAssetToStaffMemberCommand command = new(
                AssetNumber,
                viewModel.StaffId);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                Error = new()
                {
                    Error = result.Error,
                    RedirectPath = null
                };

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            return RedirectToPage();
        }

        if (viewModel.AllocationType.Equals(AllocationType.School))
        {
            if (string.IsNullOrWhiteSpace(viewModel.SchoolCode))
            {
                Error = new()
                {
                    Error = AllocationErrors.SchoolEmpty,
                    RedirectPath = null
                };

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            AllocateAssetToSchoolCommand command = new(
                AssetNumber,
                viewModel.SchoolCode);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                Error = new()
                {
                    Error = result.Error,
                    RedirectPath = null
                };

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            return RedirectToPage();
        }

        if (viewModel.AllocationType.Equals(AllocationType.CommunityMember))
        {
            if (string.IsNullOrWhiteSpace(viewModel.UserName) || string.IsNullOrWhiteSpace(viewModel.UserEmail))
            {
                Error = new()
                {
                    Error = AllocationErrors.RecipientEmpty,
                    RedirectPath = null
                };

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            AllocateAssetToCommunityMemberCommand command = new(
                AssetNumber,
                viewModel.UserName,
                viewModel.UserEmail);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                Error = new()
                {
                    Error = result.Error,
                    RedirectPath = null
                };

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            return RedirectToPage();
        }

        Error = new()
        {
            Error = AllocationErrors.UnknownType,
            RedirectPath = null
        };

        Result<AssetResponse> unknownResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
        Asset = unknownResult.Value;

        return Page();
    }
    
    public async Task<IActionResult> OnPostAjaxDeallocate() => Partial("DeallocateAssetConfirmationModal");

    public async Task<IActionResult> OnGetDeallocate()
    {
        DeallocateAssetCommand command = new(AssetNumber);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTransferAsset(TransferAssetSelection viewModel)
    {
        if (viewModel.LocationCategory.Equals(LocationCategory.CoordinatingOffice))
        {
            TransferAssetToCoordinatingOfficeCommand command = new(
                AssetNumber,
                viewModel.Room,
                true,
                viewModel.ArrivalDate);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                Error = new()
                {
                    Error = result.Error,
                    RedirectPath = null
                };

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            return RedirectToPage();
        }

        if (viewModel.LocationCategory.Equals(LocationCategory.PublicSchool))
        {
            TransferAssetToPublicSchoolCommand command = new(
                AssetNumber,
                viewModel.SchoolCode,
                true,
                viewModel.ArrivalDate);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                Error = new()
                {
                    Error = result.Error,
                    RedirectPath = null
                };

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            return RedirectToPage();
        }

        if (viewModel.LocationCategory.Equals(LocationCategory.CorporateOffice))
        {
            if (string.IsNullOrWhiteSpace(viewModel.Site))
            {
                Error = new()
                {
                    Error = LocationErrors.SiteEmpty,
                    RedirectPath = null
                };

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            TransferAssetToCorporateOfficeCommand command = new(
                AssetNumber,
                viewModel.Site,
                true,
                viewModel.ArrivalDate);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                Error = new()
                {
                    Error = result.Error,
                    RedirectPath = null
                };

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            return RedirectToPage();
        }

        if (viewModel.LocationCategory.Equals(LocationCategory.PrivateResidence))
        {
            TransferAssetToPrivateResidenceCommand command = new(
                AssetNumber,
                true,
                viewModel.ArrivalDate);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                Error = new()
                {
                    Error = result.Error,
                    RedirectPath = null
                };

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            return RedirectToPage();
        }

        Error = new()
        {
            Error = LocationErrors.UnknownCategory,
            RedirectPath = null
        };

        Result<AssetResponse> unknownResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
        Asset = unknownResult.Value;

        return Page();
    }

    public async Task<IActionResult> OnPostAddNote(AddAssetNoteSelection viewModel)
    {
        AddAssetNoteCommand command = new(
            AssetNumber,
            viewModel.Note);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = null
            };

            Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
            Asset = resetResult.Value;

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxAddSighting()
    {
        AddAssetSightingViewModel viewModel = new();

        viewModel.AssetNumber = AssetNumber;

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
            Error = new()
            {
                Error = result.Error,
                RedirectPath = null
            };

            Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
            Asset = resetResult.Value;

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateStatus(UpdateAssetStatusSelection viewModel)
    {
        UpdateAssetStatusCommand command = new(
            AssetNumber,
            viewModel.SelectedStatus);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = null
            };

            Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
            Asset = resetResult.Value;

            return Page();
        }

        return RedirectToPage();
    }
}
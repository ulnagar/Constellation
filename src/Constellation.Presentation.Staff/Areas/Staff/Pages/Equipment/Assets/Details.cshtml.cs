#nullable enable
namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Assets;

using Application.Common.PresentationModels;
using Application.Domains.AssetManagement.Assets.Commands.AddAssetNote;
using Application.Domains.AssetManagement.Assets.Commands.AllocateAsset;
using Application.Domains.AssetManagement.Assets.Commands.DeallocateAsset;
using Application.Domains.AssetManagement.Assets.Commands.SightAsset;
using Application.Domains.AssetManagement.Assets.Commands.TransferAsset;
using Application.Domains.AssetManagement.Assets.Commands.UpdateAssetStatus;
using Application.Domains.AssetManagement.Assets.Queries.GetAssetByAssetNumber;
using Application.Domains.StaffMembers.Models;
using Application.Domains.StaffMembers.Queries.GetStaffForSelectionList;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Assets.Errors;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Models.Assets.Enums;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using Shared.Components.AddAssetNote;
using Shared.Components.AllocateAsset;
using Shared.Components.TransferAsset;
using Shared.Components.UpdateAssetStatus;
using Shared.PartialViews.AddAssetSighting;
using System.Security.Claims;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
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
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Assets_Assets;
    [ViewData] public string PageTitle => Asset is null ? "Asset Details" : $"Details - {Asset.AssetNumber}";

    [BindProperty(SupportsGet = true)]
    public AssetNumber AssetNumber { get; set; }
    
    public AssetResponse? Asset { get; set; }

    public async Task OnGet()
    {
        if (string.IsNullOrWhiteSpace(AssetNumber))
        {
            _logger
                .ForContext(nameof(Error), AssetNumberErrors.Empty, true)
                .Warning("Failed to convert provided Asset Number into object");

            ModalContent = ErrorDisplay.Create(
                AssetNumberErrors.Empty,
                _linkGenerator.GetPathByPage("/Equipment/Assets/Index", values: new { area = "Staff" }));

            return;
        }

        _logger.Information("Requested to retrieve Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

        Result<AssetResponse> result = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to retrieve Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Equipment/Assets/Index", values: new { area = "Staff" }));

            return;
        }

        Asset = result.Value;
    }
    
    public async Task<IActionResult> OnPostAllocateDevice(AllocateDeviceSelection viewModel)
    {
        if (viewModel.AllocationType.Equals(AllocationType.Student))
        {
            if (viewModel.StudentId == StudentId.Empty)
            {
                ModalContent = ErrorDisplay.Create(AllocationErrors.StudentEmpty);

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            AllocateAssetToStudentCommand command = new(
                AssetNumber,
                viewModel.StudentId);

            _logger
                .ForContext(nameof(AllocateAssetToStudentCommand), command, true)
                .Information("Requested to allocate Asset with AssetNumber {AssetNumber} to student by user {User}", AssetNumber, _currentUserService.UserName);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(result.Error);

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            return RedirectToPage();
        }

        if (viewModel.AllocationType.Equals(AllocationType.Staff))
        {
            if (viewModel.StaffId == StaffId.Empty)
            {
                ModalContent = ErrorDisplay.Create(AllocationErrors.StaffEmpty);

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            AllocateAssetToStaffMemberCommand command = new(
                AssetNumber,
                viewModel.StaffId);
            
            _logger
                .ForContext(nameof(AllocateAssetToStaffMemberCommand), command, true)
                .Information("Requested to allocate Asset with AssetNumber {AssetNumber} to staff member by user {User}", AssetNumber, _currentUserService.UserName);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(result.Error);

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
                ModalContent = ErrorDisplay.Create(AllocationErrors.SchoolEmpty);

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            AllocateAssetToSchoolCommand command = new(
                AssetNumber,
                viewModel.SchoolCode);

            _logger
                .ForContext(nameof(AllocateAssetToSchoolCommand), command, true)
                .Information("Requested to allocate Asset with AssetNumber {AssetNumber} to school by user {User}", AssetNumber, _currentUserService.UserName);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(result.Error);

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
                ModalContent = ErrorDisplay.Create(AllocationErrors.RecipientEmpty);

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            AllocateAssetToCommunityMemberCommand command = new(
                AssetNumber,
                viewModel.UserName,
                viewModel.UserEmail);

            _logger
                .ForContext(nameof(AllocateAssetToCommunityMemberCommand), command, true)
                .Information("Requested to allocate Asset with AssetNumber {AssetNumber} to community member by user {User}", AssetNumber, _currentUserService.UserName);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(result.Error);

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            return RedirectToPage();
        }

        ModalContent = ErrorDisplay.Create(AllocationErrors.UnknownType);

        _logger
            .ForContext(nameof(Error), AllocationErrors.UnknownType, true)
            .Warning("Failed to determine allocation type for Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

        Result<AssetResponse> unknownResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
        Asset = unknownResult.Value;

        return Page();
    }
    
    public async Task<IActionResult> OnPostAjaxDeallocate() => Partial("DeallocateAssetConfirmationModal");

    public async Task<IActionResult> OnGetDeallocate()
    {
        DeallocateAssetCommand command = new(AssetNumber);

        _logger
            .ForContext(nameof(DeallocateAssetCommand), command, true)
            .Information("Requested to deallocate Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(result.Error);

            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to deallocate Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

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

            _logger
                .ForContext(nameof(TransferAssetToCoordinatingOfficeCommand), command, true)
                .Information("Requested to transfer Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(result.Error);

                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to transfer Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

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

            _logger
                .ForContext(nameof(TransferAssetToPublicSchoolCommand), command, true)
                .Information("Requested to transfer Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(result.Error);

                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to transfer Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

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
                ModalContent = ErrorDisplay.Create(LocationErrors.SiteEmpty);

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            TransferAssetToCorporateOfficeCommand command = new(
                AssetNumber,
                viewModel.Site,
                true,
                viewModel.ArrivalDate);

            _logger
                .ForContext(nameof(TransferAssetToCorporateOfficeCommand), command, true)
                .Information("Requested to transfer Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(result.Error);

                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to transfer Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

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

            _logger
                .ForContext(nameof(TransferAssetToPrivateResidenceCommand), command, true)
                .Information("Requested to transfer Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(result.Error);

                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to transfer Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

                Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
                Asset = resetResult.Value;

                return Page();
            }

            return RedirectToPage();
        }

        ModalContent = ErrorDisplay.Create(LocationErrors.UnknownCategory);

        _logger
            .ForContext(nameof(Error), LocationErrors.UnknownCategory, true)
            .Warning("Failed to determine location type for Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

        Result<AssetResponse> unknownResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
        Asset = unknownResult.Value;

        return Page();
    }

    public async Task<IActionResult> OnPostAddNote(AddAssetNoteSelection viewModel)
    {
        AddAssetNoteCommand command = new(
            AssetNumber,
            viewModel.Note);

        _logger
            .ForContext(nameof(AddAssetNoteCommand), command, true)
            .Information("Requested to add note to Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(result.Error);

            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to add note to Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

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
            viewModel.StaffList = new SelectList(staff.Value.OrderBy(entry => entry.Name.SortOrder), nameof(StaffSelectionListResponse.StaffId), nameof(StaffSelectionListResponse.DisplayName));
        }
        else
        {
            var guidStaffId = Guid.Parse(currentStaffId);
            StaffId staffId = StaffId.FromValue(guidStaffId);

            StaffSelectionListResponse? currentStaffMember = staff.Value.FirstOrDefault(entry => entry.StaffId == staffId);

            viewModel.StaffList = new SelectList(
                staff.Value.OrderBy(entry => entry.Name.SortOrder),
                nameof(StaffSelectionListResponse.StaffId),
                nameof(StaffSelectionListResponse.DisplayName),
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

        _logger
            .ForContext(nameof(SightAssetCommand), command, true)
            .Information("Requested to add sighting to Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(result.Error);

            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to add sighting to Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

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

        _logger
            .ForContext(nameof(UpdateAssetStatusCommand), command, true)
            .Information("Requested to update status of Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(result.Error);

            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to update status of Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

            Result<AssetResponse> resetResult = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));
            Asset = resetResult.Value;

            return Page();
        }

        return RedirectToPage();
    }
}
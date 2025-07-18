namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Assets;

using Application.Common.PresentationModels;
using Application.Domains.AssetManagement.Assets.Commands.SightAsset;
using Application.Domains.AssetManagement.Assets.Queries.ExportAssetsToExcel;
using Application.Domains.AssetManagement.Assets.Queries.GetAllActiveAssets;
using Application.Domains.AssetManagement.Assets.Queries.GetAllAssets;
using Application.Domains.AssetManagement.Assets.Queries.GetAllDisposedAssets;
using Application.Domains.StaffMembers.Models;
using Application.Domains.StaffMembers.Queries.GetStaffForSelectionList;
using Constellation.Application.Domains.AssetManagement.Assets.Enums;
using Constellation.Application.Domains.AssetManagement.Assets.Models;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using Shared.PartialViews.AddAssetSighting;
using System.Security.Claims;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }
    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Assets_Assets;
    [ViewData] public string PageTitle => "Assets List";

    [BindProperty(SupportsGet = true)] 
    public AssetFilter Filter { get; set; } = AssetFilter.Active;
    
    public IReadOnlyList<AssetListItem> Assets { get; set; }

    public async Task OnGet()
    {
        _logger
            .Information("Requested to retrieve list of Assets with filter {Filter} by user {User}", Filter, _currentUserService.UserName);

        Result<List<AssetListItem>> request = Filter switch
        {
            AssetFilter.All => await _mediator.Send(new GetAllAssetsQuery()),
            AssetFilter.Disposed => await _mediator.Send(new GetAllDisposedAssetsQuery()),
            _ => await _mediator.Send(new GetAllActiveAssetsQuery())
        };

        if (request.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(request.Error);

            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve list of Assets with filter {Filter} by user {User}", Filter, _currentUserService.UserName);

            return;
        }

        Assets = request.Value;
    }

    public async Task<IActionResult> OnGetExport()
    {
        _logger
            .Information("Requested to export list of Assets with filter {Filter} by user {User}", Filter, _currentUserService.UserName);

        Result<FileDto> file = await _mediator.Send(new ExportAssetsToExcelQuery(Filter));

        if (file.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(file.Error);

            _logger
                .ForContext(nameof(Error), file.Error, true)
                .Warning("Failed to export list of Assets with filter {Filter} by user {User}", Filter, _currentUserService.UserName);

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
            viewModel.StaffList = new SelectList(staff.Value.OrderBy(entry => entry.Name.SortOrder), nameof(StaffSelectionListResponse.StaffId), nameof(StaffSelectionListResponse.DisplayName));
        }
        else
        {
            Guid guidStaffId = Guid.Parse(currentStaffId);
            StaffId staffId = StaffId.FromValue(guidStaffId);

            StaffSelectionListResponse? currentStaffMember = staff.Value.FirstOrDefault(entry => entry.StaffId == staffId);

            viewModel.StaffList = new(
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
            .Warning("Requested to add sighting to Asset with AssetNumber {AssetNumber} by user {User}", viewModel.AssetNumber, _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(result.Error);

            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to add sighting to Asset with AssetNumber {AssetNumber} by user {User}", viewModel.AssetNumber, _currentUserService.UserName);

            return Page();
        }

        return RedirectToPage();
    }
}
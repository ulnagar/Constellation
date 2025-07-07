namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Stocktake.Sighting;

using Application.Common.PresentationModels;
using Application.Domains.AssetManagement.Stocktake.Commands.RegisterSightingFromAssetRecord;
using Application.Domains.AssetManagement.Stocktake.Queries.GetAssetForSightingConfirmation;
using Application.Models.Auth;
using Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeSightingForAsset;
using Constellation.Core.Models.Stocktake.Identifiers;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Models.Assets.Errors;
using Core.Models.Assets.Identifiers;
using Core.Models.Assets.ValueObjects;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Stocktake_Dashboard;
    [ViewData] public string PageTitle => "New Stocktake Sighting";

    [BindProperty(SupportsGet = true)]
    public StocktakeEventId Id { get; set; }

    [BindProperty]
    public AssetNumber AssetNumber { get; set; } = AssetNumber.Empty;
    
    [BindProperty]
    public string SerialNumber { get; set; } = string.Empty;

    [BindProperty]
    public int Misses { get; set; } = 0;

    public AssetSightingResponse? Asset { get; set; } = null;

    public async Task OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        if (AssetNumber == AssetNumber.Empty && string.IsNullOrWhiteSpace(SerialNumber))
        {
            ModalContent = ErrorDisplay.Create(AssetNumberErrors.Empty);

            Misses++;

            return Page();
        }

        Result<AssetSightingResponse> asset = await _mediator.Send(new GetAssetForSightingConfirmationQuery(AssetNumber, SerialNumber));

        if (asset.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(asset.Error);

            Misses++;

            return Page();
        }

        Result<StocktakeSightingForAssetResponse> sightingRecord = await _mediator.Send(new GetStocktakeSightingForAssetQuery(Id, asset.Value.AssetNumber));

        if (sightingRecord.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(sightingRecord.Error);

            Misses++;

            return Page();
        }

        if (!sightingRecord.Value.HasSighting)
        {
            Asset = asset.Value;

            return Page();
        }

        ModalContent = FeedbackDisplay.Create(
            "Stocktake Sighting",
            "This device has already been sighted. Please check the Serial Number or Asset Number and try again. If this error persists, please contact the Technology Support Team on 1300 610 733.",
            "Ok",
            "btn-success");

        Misses++;

        return Page();
    }

    public async Task<IActionResult> OnPostFinalSubmit(AssetId assetId, string comment)
    {
        if (assetId == AssetId.Empty)
        {
            Result<AssetSightingResponse> asset = await _mediator.Send(new GetAssetForSightingConfirmationQuery(AssetNumber, SerialNumber));

            if (asset.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(asset.Error);

                return Page();
            }

            Asset = asset.Value;

            return Page();
        }

        RegisterSightingFromAssetRecordCommand command = new(
            Id,
            assetId,
            comment);

        Result sighting = await _mediator.Send(command);

        if (sighting.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                sighting.Error,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Sighting/Index", values: new { area = "Staff", Id }));

            return Page();
        }

        return RedirectToPage();
    }
}
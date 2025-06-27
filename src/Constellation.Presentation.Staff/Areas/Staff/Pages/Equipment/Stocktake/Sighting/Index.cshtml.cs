namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Stocktake.Sighting;

using Application.Common.PresentationModels;
using Application.Domains.AssetManagement.Stocktake.Commands.RegisterSightingFromAssetRecord;
using Application.Domains.AssetManagement.Stocktake.Queries.GetAssetForSightingConfirmation;
using Application.Models.Auth;
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
            ModalContent = new ErrorDisplay(AssetNumberErrors.Empty);

            Misses++;

            return Page();
        }

        Result<AssetSightingResponse> asset = await _mediator.Send(new GetAssetForSightingConfirmationQuery(AssetNumber, SerialNumber));

        if (asset.IsFailure)
        {
            ModalContent = new ErrorDisplay(asset.Error);

            Misses++;

            return Page();
        }

        Asset = asset.Value;

        return Page();
    }

    public async Task<IActionResult> OnPostFinalSubmit(AssetId assetId, string comment)
    {
        if (assetId == AssetId.Empty)
        {
            Result<AssetSightingResponse> asset = await _mediator.Send(new GetAssetForSightingConfirmationQuery(AssetNumber, SerialNumber));

            if (asset.IsFailure)
            {
                ModalContent = new ErrorDisplay(asset.Error);

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
            ModalContent = new ErrorDisplay(
                sighting.Error,
                _linkGenerator.GetPathByPage("/Equipment/Stocktake/Sighting/Index", values: new { area = "Staff", Id }));

            return Page();
        }

        return RedirectToPage();
    }
}
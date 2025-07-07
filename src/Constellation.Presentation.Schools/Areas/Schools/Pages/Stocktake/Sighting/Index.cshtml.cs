namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Stocktake.Sighting;

using Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeSightingForAsset;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.AssetManagement.Stocktake.Commands.RegisterSightingFromAssetRecord;
using Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetAssetForSightingConfirmation;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Assets.Errors;
using Constellation.Core.Models.Assets.Identifiers;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Models.Stocktake.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
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
        ILogger logger,
        IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory serviceFactory)
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Stocktake;

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
            if (asset.Value.LocationCode != CurrentSchoolCode)
            {
                ModalContent = FeedbackDisplay.Create(
                    "Stocktake Sighting",
                    "This device is registered at another school! Please check the Serial Number or Asset Number and try again. If this error persists, please contact the Technology Support Team on 1300 610 733.",
                    [
                        ("Ok", "btn-success", string.Empty),
                        ("Update", "btn-warning", _linkGenerator.GetPathByPage("/Stocktake/Sighting/Update", values: new { area = "Schools", EventId = Id, AssetNumber = asset.Value.AssetNumber }))
                    ]
                );

                Misses++;

                return Page();
            }

            Asset = asset.Value;

            return Page();
        }

        if (sightingRecord.Value.SightingSchoolCode == CurrentSchoolCode)
        {
            ModalContent = FeedbackDisplay.Create(
                "Stocktake Sighting",
                "This device has already been entered. If there are mistakes, please remove the previous sighting first",
                "Ok",
                "btn-success");

            Misses++;

            return Page();
        }

        ModalContent = FeedbackDisplay.Create(
            "Stocktake Sighting",
            "This device has already been sighted at another location! Please check the Serial Number or Asset Number and try again. If this error persists, please contact the Technology Support Team on 1300 610 733.",
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
                _linkGenerator.GetPathByPage("/Stocktake/Sighting/Index", values: new { area = "Schools", Id }));

            return Page();
        }

        return RedirectToPage();
    }
}
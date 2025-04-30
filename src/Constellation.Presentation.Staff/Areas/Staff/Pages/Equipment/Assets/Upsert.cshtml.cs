namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Assets;

using Application.Assets.CreateFullAsset;
using Application.Assets.UpdateAsset;
using Application.Common.PresentationModels;
using Application.Domains.AssetManagement.Assets.Queries.GetAssetByAssetNumber;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Assets.Enums;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Models.Assets.Errors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanManageAssets)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Assets_Assets;
    [ViewData] public string PageTitle => Id is null ? "New Asset" : $"Edit Asset - {Id}";


    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(AssetNumberBinder))]
    public AssetNumber? Id { get; set; }

    [BindProperty]
    [ModelBinder(typeof(AssetNumberBinder))]
    public AssetNumber AssetNumber { get; set; }

    [BindProperty]
    [Required]
    public string SerialNumber { get; set; }

    [BindProperty]
    public string? SapEquipmentNumber { get; set; }

    [BindProperty]
    public string Manufacturer { get; set; }

    [BindProperty]
    [Required]
    public string ModelNumber { get; set; }

    [BindProperty]
    public string ModelDescription { get; set; }

    [BindProperty]
    [ModelBinder(typeof(BaseFromValueBinder))]
    public AssetCategory Category { get; set; } = AssetCategory.Student;

    [BindProperty]
    [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly? PurchaseDate { get; set; }

    [BindProperty]
    public string? PurchaseDocument { get; set; }

    [BindProperty]
    [DataType(DataType.Currency)]
    public decimal PurchaseCost { get; set; }

    [BindProperty]
    [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly? WarrantyEndDate { get; set; }

    public async Task OnGet()
    {
        if (Id is not null)
        {
            AssetNumber = Id;

            _logger
                .Information("Requested to retrieve Asset with AssetNumber {AssetNumber} for edit by user {User}", Id, _currentUserService.UserName);

            Result<AssetResponse> asset = await _mediator.Send(new GetAssetByAssetNumberQuery(Id));

            if (asset.IsFailure)
            {
                ModalContent = new ErrorDisplay(asset.Error);

                _logger
                    .ForContext(nameof(Error), asset.Error, true)
                    .Warning("Failed ot retrieve Asset with AssetNumber {AssetNumber} for edit by user {User}", Id, _currentUserService.UserName);

                return;
            }

            if (!asset.Value.Status.Equals(AssetStatus.Active) &&
                !asset.Value.Status.Equals(AssetStatus.PendingDisposal))
            {
                ModalContent = new ErrorDisplay(
                    AssetErrors.CannotUpdateDisposedItem,
                    _linkGenerator.GetPathByPage("/Equipment/Asses/Details", values: new { area = "Staff", AssetNumber = Id }));

                return;
            }

            SerialNumber = asset.Value.SerialNumber;
            SapEquipmentNumber = asset.Value.SapEquipmentNumber;
            Manufacturer = asset.Value.Manufacturer;
            ModelNumber = asset.Value.ModelNumber;
            ModelDescription = asset.Value.ModelDescription;
            Category = asset.Value.Category;
            PurchaseDate = asset.Value.PurchaseDate == DateOnly.MinValue ? null : asset.Value.PurchaseDate;
            PurchaseCost = asset.Value.PurchaseCost;
            PurchaseDocument = asset.Value.PurchaseDocument;
            WarrantyEndDate = asset.Value.WarrantyEndDate == DateOnly.MinValue ? null : asset.Value.WarrantyEndDate;
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (Id is null)
        {
            CreateFullAssetCommand createCommand = new(
                AssetNumber,
                SerialNumber,
                Manufacturer,
                ModelNumber,
                ModelDescription,
                Category,
                SapEquipmentNumber,
                PurchaseDate ?? default,
                PurchaseDocument,
                PurchaseCost,
                WarrantyEndDate ?? default);

            _logger
                .ForContext(nameof(CreateFullAssetCommand), createCommand, true)
                .Information("Requested to create new Asset by user {User}", _currentUserService.UserName);

            Result createResult = await _mediator.Send(createCommand);

            if (createResult.IsFailure)
            {
                ModalContent = new ErrorDisplay(createResult.Error);

                _logger
                    .ForContext(nameof(Error), createResult.Error, true)
                    .Warning("Failed to create new Asset by user {User}", _currentUserService.UserName);

                return Page();
            }

            return RedirectToPage("/Equipment/Assets/Details", new { area = "Staff", AssetNumber });
        }

        UpdateAssetCommand updateCommand = new(
            AssetNumber,
            SapEquipmentNumber,
            Manufacturer,
            ModelNumber,
            ModelDescription,
            PurchaseDate ?? default,
            PurchaseDocument,
            PurchaseCost,
            WarrantyEndDate ?? default);

        _logger
            .ForContext(nameof(UpdateAssetCommand), updateCommand, true)
            .Information("Requested to update Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);

        Result updateResult = await _mediator.Send(updateCommand);

        if (updateResult.IsFailure)
        {
            ModalContent = new ErrorDisplay(updateResult.Error);

            _logger
                .ForContext(nameof(Error), updateResult.Error, true)
                .Warning("Failed to update Asset with AssetNumber {AssetNumber} by user {User}", AssetNumber, _currentUserService.UserName);
            
            return Page();
        }

        return RedirectToPage("/Equipment/Assets/Details", new { area = "Staff", AssetNumber });
    }
}
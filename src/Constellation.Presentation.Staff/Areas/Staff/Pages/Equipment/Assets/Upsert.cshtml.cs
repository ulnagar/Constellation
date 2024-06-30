namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Assets;

using Application.Assets.CreateFullAsset;
using Application.Assets.UpdateAsset;
using Constellation.Application.Assets.GetAssetByAssetNumber;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Assets.Enums;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Constellation.Presentation.Staff.Areas;
using Core.Models.Assets.Errors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanManageAssets)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
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
    [ModelBinder(typeof(StringEnumerableBinder))]
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

            Result<AssetResponse> asset = await _mediator.Send(new GetAssetByAssetNumberQuery(Id));

            if (asset.IsFailure)
            {
                Error = new()
                {
                    Error = asset.Error,
                    RedirectPath = null
                };

                return;
            }

            if (!asset.Value.Status.Equals(AssetStatus.Active) &&
                !asset.Value.Status.Equals(AssetStatus.PendingDisposal))
            {
                Error = new()
                {
                    Error = AssetErrors.CannotUpdateDisposedItem,
                    RedirectPath = _linkGenerator.GetPathByPage("/Equipment/Asses/Details", values: new { area = "Staff", AssetNumber = Id })
                };

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

            Result createResult = await _mediator.Send(createCommand);

            if (createResult.IsFailure)
            {
                Error = new()
                {
                    Error = createResult.Error,
                    RedirectPath = null
                };

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

        Result updateResult = await _mediator.Send(updateCommand);

        if (updateResult.IsFailure)
        {
            Error = new()
            {
                Error = updateResult.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("/Equipment/Assets/Details", new { area = "Staff", AssetNumber });
    }
}
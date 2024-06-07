namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Assets;

using Constellation.Application.Assets.GetAssetByAssetNumber;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Assets.Enums;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Constellation.Presentation.Staff.Areas;
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

    [ViewData]
    public string ActivePage => Constellation.Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Assets_Assets;

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(AssetNumberBinder))]
    public AssetNumber? AssetNumber { get; set; }

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
    public AssetStatus Status { get; set; } = AssetStatus.Active;
    
    [BindProperty]
    public AssetCategory Category { get; set; } = AssetCategory.Student;

    [BindProperty]
    public DateOnly PurchaseDate { get; set; }
    
    [BindProperty]
    public string PurchaseDocument { get; set; }

    [BindProperty]
    public decimal PurchaseCost { get; set; }

    [BindProperty]
    public DateOnly WarrantyEndDate { get; set; }

    public async Task OnGet()
    {
        if (AssetNumber is not null)
        {
            Result<AssetResponse> asset = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));

            if (asset.IsFailure)
            {
                Error = new()
                {
                    Error = asset.Error,
                    RedirectPath = null
                };

                return;
            }

            SerialNumber = asset.Value.SerialNumber;
            SapEquipmentNumber = asset.Value.SapEquipmentNumber;
            Manufacturer = asset.Value.Manufacturer;
            ModelNumber = asset.Value.ModelNumber;
            ModelDescription = asset.Value.ModelDescription;
            Status = asset.Value.Status;
            Category = asset.Value.Category;
            PurchaseDate = asset.Value.PurchaseDate;
            PurchaseCost = asset.Value.PurchaseCost;
            PurchaseDocument = asset.Value.PurchaseDocument;
            WarrantyEndDate = asset.Value.WarrantyEndDate;
        }
    }
}
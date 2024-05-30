namespace Constellation.Presentation.Server.Areas.Equipment.Pages.Assets;

using Application.Assets.GetAssetByAssetNumber;
using Application.Models.Auth;
using BaseModels;
using Core.Models.Assets.Enums;
using Core.Models.Assets.ValueObjects;
using Core.Shared;
using Helpers.ModelBinders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [ViewData] public string ActivePage => AssetsPages.Assets;

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(AssetNumberBinder))]
    public AssetNumber AssetNumber { get; set; }

    [BindProperty]
    [Required]
    public string SerialNumber { get; set; }

    [BindProperty]
    public string SapEquipmentNumber { get; set; }
    
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
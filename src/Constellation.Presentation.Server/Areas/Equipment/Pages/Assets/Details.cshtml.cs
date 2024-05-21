#nullable enable
namespace Constellation.Presentation.Server.Areas.Equipment.Pages.Assets;

using Application.Assets.GetAssetByAssetNumber;
using Application.Models.Auth;
using BaseModels;
using Constellation.Presentation.Server.Helpers.ModelBinders;
using Core.Models.Assets.Errors;
using Core.Models.Assets.ValueObjects;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [ViewData] public string ActivePage => AssetsPages.Assets;

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(AssetNumberBinder))]
    public AssetNumber AssetNumber { get; set; }

    public AssetResponse Asset { get; set; }

    public async Task OnGet()
    {
        if (string.IsNullOrWhiteSpace(AssetNumber))
        {
            Error = new()
            {
                Error = AssetNumberErrors.Empty,
                RedirectPath = _linkGenerator.GetPathByPage("/Assets/Index", values: new { area = "Equipment" })
            };

            return;
        }

        Result<AssetResponse>? result = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Assets/Index", values: new { area = "Equipment" })
            };

            return;
        }

        Asset = result.Value;
    }
}
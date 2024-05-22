namespace Constellation.Presentation.Server.Areas.Equipment.Pages.Assets;

using Application.Assets.GetAllActiveAssets;
using Application.Assets.GetAllAssets;
using Application.Assets.GetAllDisposedAssets;
using Application.Assets.Models;
using Application.Models.Auth;
using BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => AssetsPages.Assets;

    [BindProperty(SupportsGet = true)] 
    public FilterEnum Filter { get; set; } = FilterEnum.Active;
    
    public IReadOnlyList<AssetListItem> Assets { get; set; }

    public async Task OnGet()
    {
        Result<List<AssetListItem>> request = Filter switch
        {
            FilterEnum.All => await _mediator.Send(new GetAllAssetsQuery()),
            FilterEnum.Disposed => await _mediator.Send(new GetAllDisposedAssetsQuery()),
            _ => await _mediator.Send(new GetAllActiveAssetsQuery())
        };

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return;
        }

        Assets = request.Value;
    }

    public enum FilterEnum
    {
        All,
        Active,
        Disposed
    }
}
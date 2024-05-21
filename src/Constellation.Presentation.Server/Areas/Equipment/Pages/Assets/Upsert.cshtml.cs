namespace Constellation.Presentation.Server.Areas.Equipment.Pages.Assets;

using Application.Models.Auth;
using BaseModels;
using Core.Models.Assets.Identifiers;
using Helpers.ModelBinders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    [ModelBinder(typeof(StrongIdBinder))]
    public AssetId? Id { get; set; }

    public async Task OnGet()
    {

    }
}
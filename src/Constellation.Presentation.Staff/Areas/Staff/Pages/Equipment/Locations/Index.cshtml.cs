namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Locations;

using Application.Assets.GetLocationList;
using Application.Models.Auth;
using Constellation.Core.Shared;
using Core.Models.Assets.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.ModelBinders;

[Authorize(Policy = AuthPolicies.CanManageAssets)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Assets_Locations;

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StringEnumerableBinder))]
    public LocationCategory Category { get; set; }

    public List<LocationListItem> Locations { get; set; } = new();

    public async Task OnGet()
    {
        Category ??= LocationCategory.PublicSchool;

        Result<List<LocationListItem>> result = await _mediator.Send(new GetLocationListQuery(Category));

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = null
            };

            return;
        }

        Locations = result.Value;
    }
}

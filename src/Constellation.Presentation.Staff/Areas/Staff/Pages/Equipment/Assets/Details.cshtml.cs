#nullable enable
namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Assets;

using Application.Assets.DeallocateAsset;
using Constellation.Application.Assets.GetAssetByAssetNumber;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Assets.Errors;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.ModelBinders;

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

    [ViewData] public string ActivePage => Constellation.Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Assets_Assets;

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
                RedirectPath = _linkGenerator.GetPathByPage("/Equipment/Assets/Index", values: new { area = "Staff" })
            };

            return;
        }

        Result<AssetResponse>? result = await _mediator.Send(new GetAssetByAssetNumberQuery(AssetNumber));

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Equipment/Assets/Index", values: new { area = "Staff" })
            };

            return;
        }

        Asset = result.Value;
    }

    public async Task<IActionResult> OnPostAjaxAllocate()
    {

        return Partial("DeallocateAssetConfirmationModal");
    }

    public async Task<IActionResult> OnPostAllocate()
    {


        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostAjaxDeallocate() => Partial("DeallocateAssetConfirmationModal");

    public async Task<IActionResult> OnGetDeallocate()
    {
        DeallocateAssetCommand command = new(AssetNumber);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage();
    }
}
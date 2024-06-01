namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Assets;

using Constellation.Application.Assets.Enums;
using Constellation.Application.Assets.ExportAssetsToExcel;
using Constellation.Application.Assets.GetAllActiveAssets;
using Constellation.Application.Assets.GetAllAssets;
using Constellation.Application.Assets.GetAllDisposedAssets;
using Constellation.Application.Assets.Models;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
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
    [ViewData] public string ActivePage => Constellation.Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Assets_Assets;

    [BindProperty(SupportsGet = true)] 
    public AssetFilter Filter { get; set; } = AssetFilter.Active;
    
    public IReadOnlyList<AssetListItem> Assets { get; set; }

    public async Task OnGet()
    {
        Result<List<AssetListItem>> request = Filter switch
        {
            AssetFilter.All => await _mediator.Send(new GetAllAssetsQuery()),
            AssetFilter.Disposed => await _mediator.Send(new GetAllDisposedAssetsQuery()),
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

    public async Task<IActionResult> OnGetExport()
    {
        Result<FileDto> file = await _mediator.Send(new ExportAssetsToExcelQuery(Filter));

        if (file.IsFailure)
        {
            Error = new()
            {
                Error = file.Error,
                RedirectPath = null
            };

            return Page();
        }

        return File(file.Value.FileData, file.Value.FileType, file.Value.FileName);
    }
}
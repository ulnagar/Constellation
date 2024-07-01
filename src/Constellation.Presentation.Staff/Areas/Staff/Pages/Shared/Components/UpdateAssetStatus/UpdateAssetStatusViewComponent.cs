namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.UpdateAssetStatus;

using Constellation.Core.Models.Assets.Enums;
using Microsoft.AspNetCore.Mvc;

public class UpdateAssetStatusViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(AssetStatus status)
    {
        UpdateAssetStatusSelection viewModel = new()
        {
            CurrentStatus = status
        };

        return View(viewModel);
    }
}
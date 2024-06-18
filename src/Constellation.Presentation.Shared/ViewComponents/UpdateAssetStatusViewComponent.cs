namespace Constellation.Presentation.Shared.ViewComponents;

using Core.Models.Assets.Enums;
using Microsoft.AspNetCore.Mvc;
using Pages.Shared.Components.UpdateAssetStatus;

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
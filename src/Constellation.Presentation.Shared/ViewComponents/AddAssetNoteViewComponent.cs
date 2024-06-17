namespace Constellation.Presentation.Shared.ViewComponents;

using Microsoft.AspNetCore.Mvc;
using Pages.Shared.Components.AddAssetNote;

public class AddAssetNoteViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        AddAssetNoteSelection viewModel = new();

        return View(viewModel);
    }
}
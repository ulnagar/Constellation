namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddAssetNote;

using Microsoft.AspNetCore.Mvc;

public class AddAssetNoteViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        AddAssetNoteSelection viewModel = new();

        return View(viewModel);
    }
}
namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddOfferNote;

using Microsoft.AspNetCore.Mvc;

public class AddOfferNoteViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        AddOfferNoteSelection viewModel = new();

        return View(viewModel);
    }
}

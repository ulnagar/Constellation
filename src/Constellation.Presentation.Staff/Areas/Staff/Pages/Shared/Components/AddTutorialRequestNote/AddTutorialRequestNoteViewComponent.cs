namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddTutorialRequestNote;

using Microsoft.AspNetCore.Mvc;

public class AddTutorialRequestNoteViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        AddTutorialRequestNoteSelection viewModel = new();

        return View(viewModel);
    }
}
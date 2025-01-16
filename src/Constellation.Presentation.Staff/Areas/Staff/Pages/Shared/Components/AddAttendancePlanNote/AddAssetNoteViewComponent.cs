namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddAttendancePlanNote;

using Microsoft.AspNetCore.Mvc;

public class AddAttendancePlanNoteViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        AddAttendancePlanNoteSelection viewModel = new();

        return View(viewModel);
    }
}
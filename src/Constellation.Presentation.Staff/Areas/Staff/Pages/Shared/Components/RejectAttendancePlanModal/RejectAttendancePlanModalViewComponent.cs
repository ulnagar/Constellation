namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.RejectAttendancePlanModal;

using Microsoft.AspNetCore.Mvc;

public sealed class RejectAttendancePlanModalViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        RejectAttendancePlanModalSelection viewModel = new();

        return View(viewModel);
    }
}

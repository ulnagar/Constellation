namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ApproveAttendancePlanModal;

using Microsoft.AspNetCore.Mvc;

public sealed class ApproveAttendancePlanModalViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        ApproveAttendancePlanModalSelection viewModel = new();

        return View(viewModel);
    }
}
namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.UpdateAttendancePlanStatus;

using Constellation.Core.Models.Assets.Enums;
using Constellation.Core.Models.Attendance.Enums;
using Microsoft.AspNetCore.Mvc;
using UpdateAssetStatus;

public class UpdateAttendancePlanStatusViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(AttendancePlanStatus status)
    {
        UpdateAttendancePlanStatusSelection viewModel = new()
        {
            CurrentStatus = status
        };

        return View(viewModel);
    }
}
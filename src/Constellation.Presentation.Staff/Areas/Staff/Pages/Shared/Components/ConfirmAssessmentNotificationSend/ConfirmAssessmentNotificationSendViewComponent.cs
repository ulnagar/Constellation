namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ConfirmAssessmentNotificationSend;

using Microsoft.AspNetCore.Mvc;

public sealed class ConfirmAssessmentNotificationSendViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        ConfirmAssessmentNotificationSendSelection viewModel = new();

        return View(viewModel);
    }
}

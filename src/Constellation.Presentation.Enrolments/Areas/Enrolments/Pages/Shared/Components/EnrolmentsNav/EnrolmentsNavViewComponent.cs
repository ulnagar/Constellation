namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Pages.Shared.Components.EnrolmentsNav;

using Microsoft.AspNetCore.Mvc;

public class EnrolmentsNavViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View("EnrolmentsNav");
    }
}
namespace Constellation.Presentation.Applicants.Areas.Applicants.Pages.Shared.Components.ApplicantNav;

using Microsoft.AspNetCore.Mvc;

public class ApplicantNavViewComponent : ViewComponent
{
    public IViewComponentResult Invoke() => View("ApplicantNav");
}
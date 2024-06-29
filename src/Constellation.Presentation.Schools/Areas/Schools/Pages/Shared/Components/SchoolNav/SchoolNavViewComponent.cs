namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Shared.Components.SchoolNav;

using Microsoft.AspNetCore.Mvc;

public class SchoolNavViewComponent : ViewComponent
{
    public SchoolNavViewComponent() { }

    public IViewComponentResult Invoke(string activePage) => View("SchoolNav", activePage);
}
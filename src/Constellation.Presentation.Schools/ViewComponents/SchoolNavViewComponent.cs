namespace Constellation.Presentation.Schools.ViewComponents;

using Microsoft.AspNetCore.Mvc;

public class SchoolNavViewComponent : ViewComponent
{
    public SchoolNavViewComponent() { }

    public IViewComponentResult Invoke(string activePage) => View(activePage);
}
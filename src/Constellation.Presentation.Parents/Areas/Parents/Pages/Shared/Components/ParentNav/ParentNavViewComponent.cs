namespace Constellation.Presentation.Parents.Areas.Parents.Pages.Shared.Components.ParentNav;

using Microsoft.AspNetCore.Mvc;

public class ParentNavViewComponent : ViewComponent
{
    public ParentNavViewComponent() { }

    public IViewComponentResult Invoke(string activePage) => View("ParentNav", activePage);
}
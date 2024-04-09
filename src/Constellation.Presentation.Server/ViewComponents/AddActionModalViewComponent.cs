namespace Constellation.Presentation.Server.ViewComponents;

using Microsoft.AspNetCore.Mvc;

public class AddActionModalViewComponent : ViewComponent
{
    public AddActionModalViewComponent() { }

    public IViewComponentResult Invoke() => View();
}
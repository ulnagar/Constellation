namespace Constellation.Presentation.Shared.ViewComponents;

using Microsoft.AspNetCore.Mvc;

public class AddActionModalViewComponent : ViewComponent
{
    public AddActionModalViewComponent() { }

    public IViewComponentResult Invoke() => View();
}
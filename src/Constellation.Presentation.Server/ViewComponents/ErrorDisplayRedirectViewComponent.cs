#nullable enable
namespace Constellation.Presentation.Server.ViewComponents;

using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc;

public class ErrorDisplayRedirectViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(ErrorDisplay error)
    {
        return View(error);
    }
}

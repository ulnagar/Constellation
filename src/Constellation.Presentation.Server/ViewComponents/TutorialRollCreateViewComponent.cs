namespace Constellation.Presentation.Server.ViewComponents;

using Constellation.Presentation.Server.Pages.Shared.Components.TutorialRollCreate;
using Microsoft.AspNetCore.Mvc;

public class TutorialRollCreateViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var viewModel = new TutorialRollCreateSelection();

        return View(viewModel);
    }
}

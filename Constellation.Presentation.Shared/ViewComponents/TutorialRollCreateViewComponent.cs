namespace Constellation.Presentation.Shared.ViewComponents;

using Constellation.Presentation.Shared.Pages.Shared.Components.TutorialRollCreate;
using Microsoft.AspNetCore.Mvc;

public class TutorialRollCreateViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var viewModel = new TutorialRollCreateSelection();

        return View(viewModel);
    }
}

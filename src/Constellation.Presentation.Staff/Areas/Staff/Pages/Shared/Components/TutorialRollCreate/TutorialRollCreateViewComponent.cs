namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TutorialRollCreate;

using Microsoft.AspNetCore.Mvc;

public class TutorialRollCreateViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var viewModel = new TutorialRollCreateSelection();

        return View(viewModel);
    }
}

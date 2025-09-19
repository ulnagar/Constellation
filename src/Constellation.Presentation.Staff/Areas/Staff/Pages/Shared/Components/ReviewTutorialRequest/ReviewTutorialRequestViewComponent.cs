namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ReviewTutorialRequest;

using Microsoft.AspNetCore.Mvc;

public sealed class ReviewTutorialRequestViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        ReviewTutorialRequestSelection viewModel = new();

        return View(viewModel);
    }
}

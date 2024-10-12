namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.LifecycleDetails;

using Application.Students.GetLifecycleDetailsForStudent;
using Microsoft.AspNetCore.Mvc;

public sealed class LifecycleDetailsViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(RecordLifecycleDetailsResponse viewModel)
        => View(viewModel);
}

namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.LifecycleDetails;

using Constellation.Application.Domains.Students.Queries.GetLifecycleDetailsForStudent;
using Microsoft.AspNetCore.Mvc;

public sealed class LifecycleDetailsViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(RecordLifecycleDetailsResponse viewModel)
        => View(viewModel);
}

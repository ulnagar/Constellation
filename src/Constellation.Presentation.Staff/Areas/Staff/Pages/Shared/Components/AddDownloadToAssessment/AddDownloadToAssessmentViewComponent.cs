namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddDownloadToAssessment;

using Microsoft.AspNetCore.Mvc;

public sealed class AddDownloadToAssessmentViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(
        DateTimeOffset availableFrom,
        DateTimeOffset availableTo)
    {
        AddDownloadToAssessmentSelection viewModel = new()
        {
            AvailableFrom = DateOnly.FromDateTime(availableFrom.LocalDateTime),
            AvailableTo = DateOnly.FromDateTime(availableTo.LocalDateTime)
        };

        return View(viewModel);
    }
}

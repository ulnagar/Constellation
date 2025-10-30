namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.CancelTutorial;

using Core.Abstractions.Clock;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public sealed class CancelTutorialViewComponent : ViewComponent
{
    private readonly IDateTimeProvider _dateTime;

    public CancelTutorialViewComponent(
        IDateTimeProvider dateTime)
    {
        _dateTime = dateTime;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        CancelTutorialSelection viewModel = new()
        {
            EndDate = _dateTime.Today
        };

        return View(viewModel);
    }
}

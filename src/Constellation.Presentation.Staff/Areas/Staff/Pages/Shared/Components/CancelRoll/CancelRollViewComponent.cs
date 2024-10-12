namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.CancelRoll;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public sealed class CancelRollViewComponent : ViewComponent
{
    public CancelRollViewComponent()
    {
        
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        CancelRollSelection viewModel = new();

        return View(viewModel);
    }
}

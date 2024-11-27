namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.EmailExternalReports;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public sealed class EmailExternalReportsViewComponent: ViewComponent 
{

    public async Task<IViewComponentResult> InvokeAsync()
    {
        EmailExternalReportsSelection viewModel = new();

        return View(viewModel);
    }
}

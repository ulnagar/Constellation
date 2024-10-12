namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ExportAwardNominations;

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public sealed class ExportAwardNominationsViewComponent : ViewComponent
{
    public ExportAwardNominationsViewComponent()
    {
        
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        ExportAwardNominationsSelection viewModel = new();

        return View(viewModel);
    }
}

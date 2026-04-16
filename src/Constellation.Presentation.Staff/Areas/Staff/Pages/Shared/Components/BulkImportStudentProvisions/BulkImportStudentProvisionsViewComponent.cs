namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.BulkImportStudentProvisions;

using Microsoft.AspNetCore.Mvc;

public sealed class BulkImportStudentProvisionsViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        BulkImportStudentProvisionsSelection viewModel = new();

        return View(viewModel);
    }
}

namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.UploadFileForImportStaging;

using Microsoft.AspNetCore.Mvc;

public sealed class UploadFileForImportStagingViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        UploadFileForImportStagingSelection viewModel = new();

        return View(viewModel);
    }
}

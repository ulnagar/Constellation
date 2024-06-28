#nullable enable
namespace Constellation.Presentation.Schools.Areas;

using Constellation.Application.Common.PresentationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pages.Shared.Components.SchoolSelector;

public class BasePageModel : PageModel, IBaseModel
{
    [TempData] 
    public string? CurrentSchoolCode { get; set; }

    public ErrorDisplay? Error { get; set; }

    public async Task<IActionResult> OnPostChangeSchool(SchoolSelectorViewModel viewModel)
    {
        CurrentSchoolCode = viewModel.CurrentSchoolCode;

        return RedirectToPage();
    }
}

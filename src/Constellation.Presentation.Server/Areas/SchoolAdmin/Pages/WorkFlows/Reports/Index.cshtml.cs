namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.WorkFlows.Reports;

using BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Workflows;

public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => WorkFlowPages.Reports;

    public async Task<IActionResult> OnGet()
    {
        return RedirectToPage("Attendance");
    }
}
namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.WorkFlows.Reports;

using Application.DTOs;
using Application.WorkFlows.ExportOpenCaseReport;
using BaseModels;
using Core.Shared;
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

    public void OnGet() { }

    public async Task<IActionResult> OnGetDownloadReport()
    {
        Result<FileDto> fileRequest = await _mediator.Send(new ExportOpenCaseReportQuery());

        if (fileRequest.IsFailure)
        {
            Error = new()
            {
                Error = fileRequest.Error,
                RedirectPath = null
            };

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }
}
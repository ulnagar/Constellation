namespace Constellation.Presentation.Server.Areas.Subject.Pages.SciencePracs.Reports;

using Application.DTOs;
using Application.Models.Auth;
using Application.SciencePracs.GenerateOverdueReport;
using BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.CanManageSciencePracs)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => SubjectPages.Reports;

    public async Task OnGet()
    {
        await GetClasses(_mediator);
    }

    public async Task<IActionResult> OnGetDownloadReport()
    {
        Result<FileDto> reportRequest = await _mediator.Send(new GenerateOverdueReportCommand());

        if (reportRequest.IsSuccess)
            return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);

        Error = new()
        {
            Error = reportRequest.Error,
            RedirectPath = null
        };

        await GetClasses(_mediator);
        
        return Page();
    }
}
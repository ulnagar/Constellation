namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Compliance.Wellbeing;

using Application.Compliance.ExportWellbeingReport;
using Application.Compliance.GetWellbeingReportFromSentral;
using Application.DTOs;
using Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage { get; set; } = CompliancePages.Wellbeing_Index;

    public List<SentralIncidentDetails> Data { get; set; } = new();

    public async Task OnGet() { }

    public async Task OnGetUpdate()
    {
        Result<List<SentralIncidentDetails>> request = await _mediator.Send(new GetWellbeingReportFromSentralQuery());

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return;
        }

        Data = request.Value;
    }

    public async Task<IActionResult> OnGetExport()
    {
        Result<List<SentralIncidentDetails>> dataRequest = await _mediator.Send(new GetWellbeingReportFromSentralQuery());

        if (dataRequest.IsFailure)
        {
            Error = new()
            {
                Error = dataRequest.Error,
                RedirectPath = null
            };

            return Page();
        }

        Result<FileDto> fileRequest = await _mediator.Send(new ExportWellbeingReportCommand(dataRequest.Value));

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
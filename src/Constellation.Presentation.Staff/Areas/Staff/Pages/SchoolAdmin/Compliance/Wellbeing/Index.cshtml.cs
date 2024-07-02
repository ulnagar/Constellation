namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Compliance.Wellbeing;

using Application.Common.PresentationModels;
using Application.Compliance.ExportWellbeingReport;
using Application.Compliance.GetWellbeingReportFromSentral;
using Application.DTOs;
using Application.Models.Auth;
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Compliance_Wellbeing;

    public List<SentralIncidentDetails> Data { get; set; } = new();

    public async Task OnGet() { }

    public async Task OnGetUpdate()
    {
        Result<List<SentralIncidentDetails>> request = await _mediator.Send(new GetWellbeingReportFromSentralQuery());

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(request.Error);

            return;
        }

        Data = request.Value;
    }

    public async Task<IActionResult> OnGetExport()
    {
        Result<List<SentralIncidentDetails>> dataRequest = await _mediator.Send(new GetWellbeingReportFromSentralQuery());

        if (dataRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(dataRequest.Error);

            return Page();
        }

        Result<FileDto> fileRequest = await _mediator.Send(new ExportWellbeingReportCommand(dataRequest.Value));

        if (fileRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(fileRequest.Error);

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }
}
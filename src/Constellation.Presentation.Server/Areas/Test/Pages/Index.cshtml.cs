namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Attendance.GenerateAttendanceReportForPeriod;
using Application.Compliance.GetWellbeingReportFromSentral;
using BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public List<SentralIncidentDetails> Data { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);


    }

    public async Task<IActionResult> OnPost()
    {
        var request = await _mediator.Send(new GenerateAttendanceReportForPeriodQuery("Term 4, Week 5, 2023"));

        if (request.IsFailure)
            return RedirectToPage();

        byte[] fileData = request.Value.ToArray();
        string fileName = "Attendance Report.xlsx";
        string fileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        return File(fileData, fileType, fileName);
    }
}
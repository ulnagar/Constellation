namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Attendance.GetAttendanceDataFromSentral;
using Constellation.Presentation.Server.BaseModels;
using Core.Models.Attendance;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Threading;

public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly StudentAttendanceService _service;


    public IndexModel(
        ISender mediator,
        StudentAttendanceService service)
    {
        _mediator = mediator;
        _service = service;
    }

    public List<AttendanceValue> StudentData { get; set; } = new();

    public async Task OnGetAsync()
    {
        StudentData = _service.GetAllData;
    }

    public async Task<IActionResult> OnGetRetrieveAttendance(CancellationToken cancellationToken = default)
    {
        for (int term = 1; term < 2; term++)
        {
            for (int week = 1; week < 11; week++)
            {
                Result<List<AttendanceValue>> request = await _mediator.Send(new GetAttendanceDataFromSentralQuery("2023", term.ToString(), week.ToString()), cancellationToken);

                if (request.IsSuccess)
                {
                    _service.AddItems(request.Value);
                }
            }
        }
        
        return RedirectToPage();
    }
}

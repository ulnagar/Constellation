namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Attendance.GetAttendanceDataFromSentral;
using Constellation.Presentation.Server.BaseModels;
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

    public List<StudentAttendanceData> StudentData { get; set; } = new();

    public async Task OnGetAsync()
    {
        StudentData = _service.GetAllData;
    }

    public async Task<IActionResult> OnGetRetreiveAttendance(CancellationToken cancellationToken = default)
    {
        Result<List<StudentAttendanceData>> request = await _mediator.Send(new GetAttendanceDataFromSentralQuery("2023", "2", "1"), cancellationToken);

        if (request.IsSuccess)
        {
            _service.AddItems(request.Value);
        }

        request = await _mediator.Send(new GetAttendanceDataFromSentralQuery("2023", "2", "3"), cancellationToken);

        if (request.IsSuccess)
        {
            _service.AddItems(request.Value);
        }

        request = await _mediator.Send(new GetAttendanceDataFromSentralQuery("2023", "2", "5"), cancellationToken);

        if (request.IsSuccess)
        {
            _service.AddItems(request.Value);
        }

        request = await _mediator.Send(new GetAttendanceDataFromSentralQuery("2023", "2", "7"), cancellationToken);

        if (request.IsSuccess)
        {
            _service.AddItems(request.Value);
        }

        request = await _mediator.Send(new GetAttendanceDataFromSentralQuery("2023", "2", "9"), cancellationToken);

        if (request.IsSuccess)
        {
            _service.AddItems(request.Value);
        }

        request = await _mediator.Send(new GetAttendanceDataFromSentralQuery("2023", "3", "1"), cancellationToken);

        if (request.IsSuccess)
        {
            _service.AddItems(request.Value);
        }
        
        request = await _mediator.Send(new GetAttendanceDataFromSentralQuery("2023", "3", "3"), cancellationToken);

        if (request.IsSuccess)
        {
            _service.AddItems(request.Value);
        }
        
        request = await _mediator.Send(new GetAttendanceDataFromSentralQuery("2023", "3", "5"), cancellationToken);

        if (request.IsSuccess)
        {
            _service.AddItems(request.Value);
        }
        
        request = await _mediator.Send(new GetAttendanceDataFromSentralQuery("2023", "3", "7"), cancellationToken);

        if (request.IsSuccess)
        {
            _service.AddItems(request.Value);
        }
        
        request = await _mediator.Send(new GetAttendanceDataFromSentralQuery("2023", "3", "9"), cancellationToken);

        if (request.IsSuccess)
        {
            _service.AddItems(request.Value);
        }

        return RedirectToPage();
    }
}

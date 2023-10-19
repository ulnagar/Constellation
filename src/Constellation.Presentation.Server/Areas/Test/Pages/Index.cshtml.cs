namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Attendance.GetAttendanceDataFromSentral;
using Constellation.Presentation.Server.BaseModels;
using Core.Shared;
using MediatR;
using System.Threading;

public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;


    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public List<StudentAttendanceData> StudentData { get; set; } = new();

    public async Task OnGetAsync()
    {
        
    }

    public async Task OnGetRetreiveAttendance(CancellationToken cancellationToken = default)
    {
        Result<List<StudentAttendanceData>> request = await _mediator.Send(new GetAttendanceDataFromSentralQuery("2023", "3", "1"), cancellationToken);

        if (request.IsSuccess)
        {
            StudentData = request.Value;
        }
    }
}

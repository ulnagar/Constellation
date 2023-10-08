namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Attendance.GetAttendanceDataFromSentral;
using Application.Interfaces.Gateways;
using Application.Interfaces.Services;
using Constellation.Presentation.Server.BaseModels;
using System.Threading;

public class IndexModel : BasePageModel
{
    private readonly ISentralGateway _gateway;
    private readonly IExcelService _excelService;


    public IndexModel(
        ISentralGateway gateway,
        IExcelService excelService)
    {
        _gateway = gateway;
        _excelService = excelService;
    }


    public async Task OnGetAsync()
    {
        
    }

    public async Task OnGetRetreiveAttendance(CancellationToken cancellationToken = default)
    {
        SystemAttendanceData sentralData = await _gateway.GetAttendancePercentages();

        List<StudentAttendanceData> systemData = await _excelService.ReadSystemAttendanceData(
            new List<StudentAttendanceData>(), 
            sentralData, 
            cancellationToken);



    }

}

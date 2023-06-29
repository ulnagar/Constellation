namespace Constellation.Application.Attendance.GetValidAttendanceReportDates;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetValidAttendenceReportDatesQueryHandler 
    : IQueryHandler<GetValidAttendenceReportDatesQuery, List<ValidAttendenceReportDate>>
{
    private readonly ISentralGateway _sentralGateway;

    public GetValidAttendenceReportDatesQueryHandler(ISentralGateway sentralGateway)
    {
        _sentralGateway = sentralGateway;
    }

    public async Task<Result<List<ValidAttendenceReportDate>>> Handle(GetValidAttendenceReportDatesQuery request, CancellationToken cancellationToken) =>
        await _sentralGateway.GetValidAttendanceReportDatesFromCalendar(DateTime.Today.Year.ToString());
}

namespace Constellation.Application.Features.Attendance.Queries;

using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Gateways;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class GetValidAttendenceReportDatesQuery : IRequest<IList<ValidAttendenceReportDate>>
{
}

public class GetValidAttendenceReportDatesQueryHandler : IRequestHandler<GetValidAttendenceReportDatesQuery, IList<ValidAttendenceReportDate>>
{
    private readonly ISentralGateway _sentralGateway;

    public GetValidAttendenceReportDatesQueryHandler() { }
    public GetValidAttendenceReportDatesQueryHandler(ISentralGateway sentralGateway)
    {
        _sentralGateway = sentralGateway;
    }

    public async Task<IList<ValidAttendenceReportDate>> Handle(GetValidAttendenceReportDatesQuery request, CancellationToken cancellationToken)
    {
        if (_sentralGateway is null)
        {
            return new List<ValidAttendenceReportDate>();
        }

        return await _sentralGateway.GetValidAttendanceReportDatesFromCalendar(DateTime.Today.Year.ToString());
    }
}

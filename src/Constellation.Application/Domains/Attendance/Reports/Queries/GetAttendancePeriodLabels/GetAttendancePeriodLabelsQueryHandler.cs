namespace Constellation.Application.Domains.Attendance.Reports.Queries.GetAttendancePeriodLabels;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models.Attendance.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAttendancePeriodLabelsQueryHandler
: IQueryHandler<GetAttendancePeriodLabelsQuery, List<string>>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IDateTimeProvider _dateTime;

    public GetAttendancePeriodLabelsQueryHandler(
        IAttendanceRepository attendanceRepository,
        IDateTimeProvider dateTime)
    {
        _attendanceRepository = attendanceRepository;
        _dateTime = dateTime;
    }
    
    public async Task<Result<List<string>>> Handle(GetAttendancePeriodLabelsQuery request, CancellationToken cancellationToken)
    {
        return await _attendanceRepository.GetPeriodNames(_dateTime.CurrentYear, cancellationToken);
    }
}


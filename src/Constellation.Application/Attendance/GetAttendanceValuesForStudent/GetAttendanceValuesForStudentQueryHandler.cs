namespace Constellation.Application.Attendance.GetAttendanceValuesForStudent;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models.Attendance;
using Core.Models.Attendance.Repositories;
using Core.Models.Students.Errors;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAttendanceValuesForStudentQueryHandler
: IQueryHandler<GetAttendanceValuesForStudentQuery, List<AttendanceValue>>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetAttendanceValuesForStudentQueryHandler(
        IAttendanceRepository attendanceRepository,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _attendanceRepository = attendanceRepository;
        _dateTime = dateTime;
        _logger = logger.ForContext<GetAttendanceValuesForStudentQuery>();
    }

    public async Task<Result<List<AttendanceValue>>> Handle(GetAttendanceValuesForStudentQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.StudentId))
        {
            _logger
                .ForContext(nameof(GetAttendanceValuesForStudentQuery), request, true)
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Warning("Failed to retrieve Attendance Values for student");

            return Result.Failure<List<AttendanceValue>>(StudentErrors.InvalidId);
        }

        List<AttendanceValue> values = await _attendanceRepository.GetAllForStudent(_dateTime.CurrentYear, request.StudentId, cancellationToken);

        return values;
    }
}

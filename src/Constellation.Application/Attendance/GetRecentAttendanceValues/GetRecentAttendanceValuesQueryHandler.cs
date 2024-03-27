namespace Constellation.Application.Attendance.GetRecentAttendanceValues;

using Abstractions.Messaging;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Attendance;
using Core.Models.Attendance.Repositories;
using Core.Models.Students;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal class GetRecentAttendanceValuesQueryHandler
: IQueryHandler<GetRecentAttendanceValuesQuery, List<AttendanceValueResponse>>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetRecentAttendanceValuesQueryHandler(
        IAttendanceRepository attendanceRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
        _logger = logger.ForContext<GetRecentAttendanceValuesQuery>();
    }

    public async Task<Result<List<AttendanceValueResponse>>> Handle(GetRecentAttendanceValuesQuery request, CancellationToken cancellationToken)
    {
        List<AttendanceValueResponse> response = new();

        List<Student> students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);

        List<AttendanceValue> values = await _attendanceRepository.GetAllRecent(cancellationToken);

        foreach (AttendanceValue value in values)
        {
            Student student = students.FirstOrDefault(entry => entry.StudentId == value.StudentId);

            response.Add(new(
                value.StudentId,
                student?.GetName(),
                value.Grade,
                student?.School.Name,
                value.PeriodLabel,
                value.PerDayYearToDatePercentage,
                value.PerDayWeekPercentage,
                value.PerMinuteYearToDatePercentage,
                value.PerMinuteWeekPercentage));
        }

        return response;
    }
}

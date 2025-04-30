namespace Constellation.Application.Domains.Attendance.Reports.Queries.GenerateCustomReportForPeriod;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Attendance.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Students.Enums;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThirdPartyConsent.GetRequiredApplicationsForStudent;

internal sealed class GenerateCustomReportForPeriodQueryHandler
: IQueryHandler<GenerateCustomReportForPeriodQuery, MemoryStream>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IExcelService _excelService;
    private readonly ILogger _logger;

    public GenerateCustomReportForPeriodQueryHandler(
        IStudentRepository studentRepository,
        IAttendanceRepository attendanceRepository,
        IExcelService excelService,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _attendanceRepository = attendanceRepository;
        _excelService = excelService;
        _logger = logger
            .ForContext<GenerateCustomReportForPeriodQuery>();
    }

    public async Task<Result<MemoryStream>> Handle(GenerateCustomReportForPeriodQuery request, CancellationToken cancellationToken)
    {
        List<ExportRecord> records = new();

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        List<AttendanceValue> values = await _attendanceRepository.GetForReportWithTitle(request.PeriodLabel, cancellationToken);

        foreach (AttendanceValue value in values)
        {
            Student student = students.FirstOrDefault(student => student.Id == value.StudentId);

            if (student is null)
                continue;

            SchoolEnrolment enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            records.Add(new(
                student.StudentReferenceNumber,
                student.Name,
                enrolment.Grade,
                enrolment.SchoolName,
                student.IndigenousStatus,
                request.ValueType switch
                {
                    "YTDDay" => value.PerDayYearToDatePercentage,
                    "YTDMin" => value.PerMinuteYearToDatePercentage,
                    "WeekDay" => value.PerDayWeekPercentage,
                    "WeekMin" => value.PerMinuteWeekPercentage,
                    _ => decimal.MinValue
                }));
        }
        
        MemoryStream result = await _excelService.CreateCustomAttendanceReport(request.PeriodLabel, records, cancellationToken);

        return result;
    }
}

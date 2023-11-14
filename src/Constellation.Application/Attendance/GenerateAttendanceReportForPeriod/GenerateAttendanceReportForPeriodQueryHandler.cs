namespace Constellation.Application.Attendance.GenerateAttendanceReportForPeriod;

using Abstractions.Messaging;
using Constellation.Core.Models.Attendance;
using Core.Abstractions.Repositories;
using Core.Models.Absences;
using Core.Models.Attendance.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.Students;
using Core.Shared;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GenerateAttendanceReportForPeriodQueryHandler
: IQueryHandler<GenerateAttendanceReportForPeriodQuery, MemoryStream>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IExcelService _excelService;
    private readonly ILogger _logger;

    public GenerateAttendanceReportForPeriodQueryHandler(
        IStudentRepository studentRepository,
        IAbsenceRepository absenceRepository,
        IAttendanceRepository attendanceRepository,
        IOfferingRepository offeringRepository,
        IExcelService excelService,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _absenceRepository = absenceRepository;
        _attendanceRepository = attendanceRepository;
        _offeringRepository = offeringRepository;
        _excelService = excelService;
        _logger = logger;
    }

    public async Task<Result<MemoryStream>> Handle(GenerateAttendanceReportForPeriodQuery request, CancellationToken cancellationToken)
    {
        List<AttendanceRecord> records = new();
        List<AbsenceRecord> absenceRecords = new();

        List<Student> students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);
        List<Offering> offerings = await _offeringRepository.GetAllActive(cancellationToken);

        List<AttendanceValue> values = await _attendanceRepository.GetForReportWithTitle(request.PeriodLabel, cancellationToken);

        DateOnly startDate;
        DateOnly endDate;

        foreach (AttendanceValue value in values)
        {
            Student student = students.FirstOrDefault(student => student.StudentId == value.StudentId)!;

            records.Add(new(
                value.StudentId,
                student.GetName(),
                value.Grade,
                value.PerMinuteYearToDatePercentage,
                value.PerMinuteYearToDatePercentage >= 90 ? "90% - 100% Attendance" :
                    value.PerMinuteYearToDatePercentage >= 75 ? "75% - 90% Attendance" :
                    value.PerMinuteYearToDatePercentage >= 50 ? "50% - 75% Attendance" :
                    "Below 50% Attendance"));

            startDate = value.StartDate;
            endDate = value.EndDate;
        }

        foreach (AttendanceRecord record in records.Where(record => record.Group == "Below 50% Attendance"))
        {
            List<Absence> absences = await _absenceRepository.GetForStudentFromDateRange(record.StudentId, startDate, endDate, cancellationToken);

            foreach (Absence absence in absences)
            {
                Offering offering = offerings.FirstOrDefault(offering => offering.Id == absence.OfferingId);

                absenceRecords.Add(new(
                    absence.StudentId,
                    $"{absence.AbsenceReason} ({absence.Type.Value})",
                    absence.Date,
                    offering?.Name.ToString(),
                    1));
            }
        }

        foreach (AttendanceRecord record in records.Where(record => record.Group == "50% - 75% Attendance"))
        {
            List<Absence> absences =
                await _absenceRepository.GetForStudentFromDateRange(record.StudentId, startDate, endDate,
                    cancellationToken);

            foreach (Absence absence in absences)
            {
                Offering offering = offerings.FirstOrDefault(offering => offering.Id == absence.OfferingId);

                absenceRecords.Add(new(
                    absence.StudentId,
                    $"{absence.AbsenceReason} ({absence.Type.Value})",
                    absence.Date,
                    offering?.Name.ToString(),
                    2));
            }
        }

        MemoryStream result = await _excelService.CreateStudentAttendanceReport(values.First().PeriodLabel, records, absenceRecords, cancellationToken);

        return result;
    }
}

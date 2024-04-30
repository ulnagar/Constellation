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

        DateOnly startDate = values.First().StartDate;
        DateOnly endDate = values.First().EndDate;

        List<DateOnly> reportDates = await _attendanceRepository.GetPeriodEndDates(endDate.Year, cancellationToken);
        int historyPosition = reportDates.IndexOf(endDate);
        DateOnly historyBoundary = reportDates[historyPosition - 4];
        
        foreach (AttendanceValue value in values)
        {
            Student student = students.FirstOrDefault(student => student.StudentId == value.StudentId);

            if (student is null)
                continue;

            List<AttendanceValue> recentHistory = await _attendanceRepository.GetForStudentBetweenDates(student.StudentId, historyBoundary, endDate, cancellationToken);

            records.Add(new(
                value.StudentId,
                student.GetName(),
                value.Grade,
                value.PerMinuteYearToDatePercentage,
                value.PerMinuteYearToDatePercentage >= 90 ? "90% - 100% Attendance" :
                    value.PerMinuteYearToDatePercentage >= 75 ? "75% - 90% Attendance" :
                    value.PerMinuteYearToDatePercentage >= 50 ? "50% - 75% Attendance" :
                    "Below 50% Attendance",
                ImprovingText(recentHistory),
                DecliningText(recentHistory)));

            startDate = value.StartDate;
            endDate = value.EndDate;
        }

        foreach (AttendanceRecord record in records.Where(record => record.Group == "Below 50% Attendance"))
        {
            List<Absence> absences = await _absenceRepository.GetForStudentFromDateRange(record.StudentId, startDate, endDate, cancellationToken);

            foreach (Absence absence in absences)
            {
                if (absence.Type.Equals(AbsenceType.Partial) && absence.AbsenceReason.Equals(AbsenceReason.SharedEnrolment))
                    continue;

                Offering offering = offerings.FirstOrDefault(offering => offering.Id == absence.OfferingId);

                string reason = absence.Type.Equals(AbsenceType.Partial)
                    ? $"{absence.AbsenceReason} ({absence.Type.Value} - {absence.AbsenceLength} min)"
                    : $"{absence.AbsenceReason} ({absence.Type.Value})";

                absenceRecords.Add(new(
                    absence.StudentId,
                    reason,
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
                if (absence.Type.Equals(AbsenceType.Partial) && absence.AbsenceReason.Equals(AbsenceReason.SharedEnrolment))
                    continue;

                Offering offering = offerings.FirstOrDefault(offering => offering.Id == absence.OfferingId);

                string reason = absence.Type.Equals(AbsenceType.Partial)
                    ? $"{absence.AbsenceReason} ({absence.Type.Value} - {absence.AbsenceLength} min)"
                    : $"{absence.AbsenceReason} ({absence.Type.Value})";
                
                absenceRecords.Add(new(
                    absence.StudentId,
                    reason,
                    absence.Date,
                    offering?.Name.ToString(),
                    2));
            }
        }

        MemoryStream result = await _excelService.CreateStudentAttendanceReport(values.First().PeriodLabel, records, absenceRecords, cancellationToken);

        return result;
    }

    private decimal NormalisedPerMinuteWeekPercentage(AttendanceValue value)
    {
        if (value.PerMinuteWeekPercentage == 0 && value.PerDayWeekPercentage == 100)
            return value.PerDayWeekPercentage;

        return value.PerMinuteWeekPercentage;
    }

    private string DecliningText(List<AttendanceValue> values)
    {
        // Compare trend data
        // Is the most recent attendance figure 40% or less? If so, attendance is declining.
        //if (NormalisedPerMinuteWeekPercentage(values.Last()) <= 40)
        //    return true;

        // Is the more recent attendance figure 100%? If so, attendance cannot be declining.
        if (NormalisedPerMinuteWeekPercentage(values.Last()) == 100)
            return string.Empty;

        // Comparing Week 0 with Week 4, is the overall difference more than a decline of 30%? If so, attendance is declining.
        decimal difference = NormalisedPerMinuteWeekPercentage(values.Last()) - NormalisedPerMinuteWeekPercentage(values.First());
        decimal percentage = NormalisedPerMinuteWeekPercentage(values.First()) == 0 
            ? difference
            : difference / NormalisedPerMinuteWeekPercentage(values.First()) * 100;

        if (percentage < -30m)
            return $"{(percentage * -1):F}% 5 Week Decrease{Environment.NewLine}{values.First().PeriodLabel} - {NormalisedPerMinuteWeekPercentage(values.First())}{Environment.NewLine}{values.Last().PeriodLabel} - {NormalisedPerMinuteWeekPercentage(values.Last())}";

        // Comparing each Week to the preceding one, is there an ongoing decline of more than 5%? If so, attendance is declining.
        for (int i = 0; i < (values.Count - 1); i++)
        {
            decimal current = NormalisedPerMinuteWeekPercentage(values[i]);
            decimal next = NormalisedPerMinuteWeekPercentage(values[i + 1]);

            difference = next - current;
            percentage = current == 0 
                ? current 
                : difference / current * 100;

            // If the percentage difference is an increase in attendance,
            // or the delta is less than a decrease of 5%, attendance is not declining.
            if (percentage > -5m)
                return string.Empty;
        }

        // Attendance is declining!
        return values.Aggregate("5 Week Decrease > 5% per week" + Environment.NewLine, (current, entry) => current + ($"{entry.PeriodLabel} - {NormalisedPerMinuteWeekPercentage(entry)}" + Environment.NewLine));
    }

    private string ImprovingText(List<AttendanceValue> values)
    {
        // Compare trend data
        // Is the more distant attendance figure 100%? If so, attendance cannot be improving.
        if (NormalisedPerMinuteWeekPercentage(values.First()) == 100)
            return string.Empty;

        // Comparing Week 0 with Week 4, is the overall difference more than an increase of 30%? If so, attendance is increasing.
        decimal difference = NormalisedPerMinuteWeekPercentage(values.Last()) - NormalisedPerMinuteWeekPercentage(values.First());
        decimal percentage = NormalisedPerMinuteWeekPercentage(values.First()) == 0
            ? difference
            : difference / NormalisedPerMinuteWeekPercentage(values.First()) * 100;

        if (percentage > 30m)
            return $"{percentage:F}% 5 Week Increase{Environment.NewLine}{values.First().PeriodLabel} - {NormalisedPerMinuteWeekPercentage(values.First())}{Environment.NewLine}{values.Last().PeriodLabel} - {NormalisedPerMinuteWeekPercentage(values.Last())}";

        // Comparing each Week to the preceding one, is there an ongoing increase of more than 10%? If so, attendance is increasing.
        for (int i = 0; i < (values.Count - 1); i++)
        {
            decimal current = NormalisedPerMinuteWeekPercentage(values[i]);
            decimal next = NormalisedPerMinuteWeekPercentage(values[i + 1]);

            // If the student has 100% attendance for the next period, any percentage difference is irrelevant
            // Even if the percentage is less than 10%, there is no way to get better attendance than 100%
            // This also deals with cases where each period is 100% and the delta between them is zero.
            if (next == 100)
                continue;

            difference = next - current;
            percentage = current == 0
                ? current
                : difference / current * 100;

            // If the percentage difference is a decrease in attendance,
            // or the delta is less than 10%, attendance is not increasing.
            if (percentage < 10m)
                return string.Empty;
        }

        // Attendance is increasing!
        return values.Aggregate("5 Week Increase > 10% per week" + Environment.NewLine, (current, entry) => current + ($"{entry.PeriodLabel} - {NormalisedPerMinuteWeekPercentage(entry)}" + Environment.NewLine));
    }
}
namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.Attendance.GenerateAttendanceReportForStudent;
using Constellation.Core.Models.Students;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public interface IExportService
{
     Task<MemoryStream> CreateAttendanceReport(Student student, DateOnly startDate, List<DateOnly> excludedDates, List<AttendanceAbsenceDetail> absences, List<AttendanceDateDetail> dates, CancellationToken cancellationToken = default);
}
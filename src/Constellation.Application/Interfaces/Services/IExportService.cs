namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.Attendance.GenerateAttendanceReportForStudent;
using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public interface IExportService
{
    Task<List<InterviewExportDto>> CreatePTOExport(List<Student> students, bool perFamily, bool residentialFamilyOnly, CancellationToken cancellationToken = default);
    Task<MemoryStream> CreateAttendanceReport(Student student, DateOnly startDate, List<DateOnly> excludedDates, List<AttendanceAbsenceDetail> absences, List<AttendanceDateDetail> dates, CancellationToken cancellationToken = default);
}
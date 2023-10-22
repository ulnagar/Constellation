namespace Constellation.Application.Attendance.GenerateAttendanceReportForStudent;

using Constellation.Application.Abstractions.Messaging;
using DTOs;
using System;

public sealed record GenerateAttendanceReportForStudentQuery(
    string StudentId,
    DateOnly StartDate,
    DateOnly EndDate)
    : IQuery<FileDto>;

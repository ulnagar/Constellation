namespace Constellation.Application.Attendance.GenerateAttendanceReportForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models;
using System;

public sealed record GenerateAttendanceReportForStudentQuery(
    string StudentId,
    DateOnly StartDate,
    DateOnly EndDate)
    : IQuery<StoredFile>;

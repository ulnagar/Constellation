namespace Constellation.Application.Attendance.GenerateAttendanceReportForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using DTOs;
using System;

public sealed record GenerateAttendanceReportForStudentQuery(
    StudentId StudentId,
    DateOnly StartDate,
    DateOnly EndDate)
    : IQuery<FileDto>;

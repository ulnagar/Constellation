﻿namespace Constellation.Application.Attendance.GenerateAttendanceReportForPeriod;

using Core.Enums;
using Core.ValueObjects;

public sealed record AttendanceRecord(
    string StudentId,
    Name StudentName,
    Grade Grade,
    decimal Percentage,
    string Group,
    string Improvement,
    string Decline);
﻿namespace Constellation.Application.Attendance.GetRecentAttendanceValues;

using Core.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;

public sealed record AttendanceValueResponse(
    StudentId StudentId,
    StudentReferenceNumber StudentReferenceNumber,
    Name? StudentName,
    Grade StudentGrade,
    string SchoolName,
    string PeriodLabel,
    decimal PerDayYearToDatePercentage,
    decimal PerDayWeekPercentage,
    decimal PerMinuteYearToDatePercentage,
    decimal PerMinuteWeekPercentage);
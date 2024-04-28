namespace Constellation.Application.Attendance.GetAttendanceTrendValues;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAttendanceTrendValuesQuery()
    : IQuery<List<AttendanceTrend>>;
namespace Constellation.Application.Domains.Attendance.Reports.Queries.GetAttendanceTrendValues;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAttendanceTrendValuesQuery()
    : IQuery<List<AttendanceTrend>>;
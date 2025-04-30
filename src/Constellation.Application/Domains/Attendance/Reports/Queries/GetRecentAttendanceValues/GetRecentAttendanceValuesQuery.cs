namespace Constellation.Application.Domains.Attendance.Reports.Queries.GetRecentAttendanceValues;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetRecentAttendanceValuesQuery()
    : IQuery<List<AttendanceValueResponse>>;
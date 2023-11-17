namespace Constellation.Application.Attendance.GetRecentAttendanceValues;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetRecentAttendanceValuesQuery()
    : IQuery<List<AttendanceValueResponse>>;
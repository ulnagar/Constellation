namespace Constellation.Application.Attendance.GetAttendancePeriodLabels;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAttendancePeriodLabelsQuery()
    : IQuery<List<string>>;
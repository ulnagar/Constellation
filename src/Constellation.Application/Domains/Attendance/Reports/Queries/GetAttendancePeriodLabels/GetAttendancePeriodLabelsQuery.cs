namespace Constellation.Application.Domains.Attendance.Reports.Queries.GetAttendancePeriodLabels;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAttendancePeriodLabelsQuery()
    : IQuery<List<string>>;
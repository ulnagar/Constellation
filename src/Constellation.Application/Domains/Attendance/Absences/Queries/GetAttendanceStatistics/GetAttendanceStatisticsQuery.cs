namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAttendanceStatistics;

using Abstractions.Messaging;
using Constellation.Application.Domains.Attendance.Absences.Models;
using Models;

public sealed record GetAttendanceStatisticsQuery
    : IQuery<AttendanceStatisticsResponse>;

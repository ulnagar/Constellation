namespace Constellation.Application.Attendance.GetValidAttendanceReportDates;

using Constellation.Application.Abstractions.Messaging;
using MediatR;
using System.Collections.Generic;

public sealed record GetValidAttendenceReportDatesQuery()
    : IQuery<List<ValidAttendenceReportDate>>;
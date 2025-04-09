namespace Constellation.Application.Attendance.GetAttendanceDataFromSentral;

using Abstractions.Messaging;
using Constellation.Core.Enums;
using Core.Models.Attendance;
using System.Collections.Generic;

public sealed record GetAttendanceDataFromSentralQuery(
    string Year,
    SchoolTerm Term,
    SchoolWeek Week)
    : IQuery<List<AttendanceValue>>;
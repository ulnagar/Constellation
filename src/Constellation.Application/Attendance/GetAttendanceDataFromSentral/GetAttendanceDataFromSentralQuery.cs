namespace Constellation.Application.Attendance.GetAttendanceDataFromSentral;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAttendanceDataFromSentralQuery(
    string Year,
    string Term,
    string Week)
    : IQuery<List<StudentAttendanceData>>;
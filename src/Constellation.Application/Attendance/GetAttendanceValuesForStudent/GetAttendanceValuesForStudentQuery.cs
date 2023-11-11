namespace Constellation.Application.Attendance.GetAttendanceValuesForStudent;

using Abstractions.Messaging;
using Core.Models.Attendance;
using System.Collections.Generic;

public sealed record GetAttendanceValuesForStudentQuery(
    string StudentId)
    : IQuery<List<AttendanceValue>>;
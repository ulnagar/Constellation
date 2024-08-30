namespace Constellation.Application.Attendance.GetAttendanceValuesForStudent;

using Abstractions.Messaging;
using Core.Models.Attendance;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetAttendanceValuesForStudentQuery(
    StudentId StudentId)
    : IQuery<List<AttendanceValue>>;
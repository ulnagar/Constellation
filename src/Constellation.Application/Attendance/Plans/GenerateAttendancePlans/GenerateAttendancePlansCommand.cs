namespace Constellation.Application.Attendance.Plans.GenerateAttendancePlans;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Students.Identifiers;

public sealed record GenerateAttendancePlansCommand(
    StudentId StudentId,
    string SchoolCode,
    Grade? Grade)
    : ICommand;
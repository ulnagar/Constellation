namespace Constellation.Application.Domains.Attendance.Plans.Commands.GenerateAttendancePlans;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Identifiers;
using Core.Models.Students.Identifiers;

public sealed record GenerateAttendancePlansCommand(
    StudentId StudentId,
    SchoolCode SchoolCode,
    Grade? Grade)
    : ICommand;
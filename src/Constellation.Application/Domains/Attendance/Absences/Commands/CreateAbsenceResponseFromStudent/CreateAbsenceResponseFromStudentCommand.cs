namespace Constellation.Application.Domains.Attendance.Absences.Commands.CreateAbsenceResponseFromStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Absences.Identifiers;
using Constellation.Core.Models.Students.Identifiers;

public sealed record CreateAbsenceResponseFromStudentCommand(
    AbsenceId AbsenceId,
    StudentId StudentId,
    string Explanation)
    : ICommand;
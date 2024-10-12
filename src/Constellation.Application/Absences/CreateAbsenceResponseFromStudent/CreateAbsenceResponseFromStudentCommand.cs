namespace Constellation.Application.Absences.CreateAbsenceResponseFromStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using Core.Models.Students.Identifiers;

public sealed record CreateAbsenceResponseFromStudentCommand(
    AbsenceId AbsenceId,
    StudentId StudentId,
    string Explanation)
    : ICommand;
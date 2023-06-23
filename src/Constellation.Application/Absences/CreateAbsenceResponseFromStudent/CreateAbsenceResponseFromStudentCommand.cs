namespace Constellation.Application.Absences.CreateAbsenceResponseFromStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record CreateAbsenceResponseFromStudentCommand(
    AbsenceId AbsenceId,
    string StudentId,
    string Explanation)
    : ICommand;
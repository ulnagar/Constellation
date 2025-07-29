namespace Constellation.Application.Domains.Attendance.Absences.Commands.RejectStudentExplanation;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Absences.Identifiers;

public sealed record RejectStudentExplanationCommand(
    AbsenceId AbsenceId,
    AbsenceResponseId ResponseId,
    string UserEmail,
    string Comment)
    : ICommand;

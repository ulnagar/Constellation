namespace Constellation.Application.Absences.RejectStudentExplanation;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record RejectStudentExplanationCommand(
    AbsenceId AbsenceId,
    AbsenceResponseId ResponseId,
    string UserEmail,
    string Comment)
    : ICommand;

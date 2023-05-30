namespace Constellation.Application.Absences.VerifyStudenExplanation;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record VerifyStudentExplanationCommand(
    AbsenceId AbsenceId,
    AbsenceResponseId ResponseId,
    string UserEmail,
    string Comment)
    : ICommand;
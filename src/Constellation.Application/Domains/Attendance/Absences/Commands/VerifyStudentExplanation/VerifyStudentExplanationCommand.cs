namespace Constellation.Application.Domains.Attendance.Absences.Commands.VerifyStudentExplanation;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Absences.Identifiers;

public sealed record VerifyStudentExplanationCommand(
    AbsenceId AbsenceId,
    AbsenceResponseId ResponseId,
    string UserEmail,
    string Comment)
    : ICommand;
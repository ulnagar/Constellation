namespace Constellation.Application.Domains.Attendance.Absences.Commands.ConvertResponseToAbsenceExplanation;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record ConvertResponseToAbsenceExplanationCommand(
    AbsenceId AbsenceId,
    AbsenceResponseId ResponseId)
    : ICommand<AbsenceExplanation>;
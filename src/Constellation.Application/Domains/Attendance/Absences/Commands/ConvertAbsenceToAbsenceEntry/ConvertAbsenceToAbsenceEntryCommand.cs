namespace Constellation.Application.Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record  ConvertAbsenceToAbsenceEntryCommand(
    AbsenceId AbsenceId)
    : ICommand<AbsenceEntry>;
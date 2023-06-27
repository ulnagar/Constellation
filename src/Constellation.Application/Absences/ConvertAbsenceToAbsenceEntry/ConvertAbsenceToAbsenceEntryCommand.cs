namespace Constellation.Application.Absences.ConvertAbsenceToAbsenceEntry;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record  ConvertAbsenceToAbsenceEntryCommand(
    AbsenceId AbsenceId)
    : ICommand<AbsenceEntry>;
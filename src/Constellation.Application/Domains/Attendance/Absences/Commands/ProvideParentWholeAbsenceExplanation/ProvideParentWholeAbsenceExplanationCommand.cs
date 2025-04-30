namespace Constellation.Application.Domains.Attendance.Absences.Commands.ProvideParentWholeAbsenceExplanation;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record ProvideParentWholeAbsenceExplanationCommand(
    AbsenceId AbsenceId,
    string Comment) : ICommand
{
    public string ParentEmail { get; set; }
}


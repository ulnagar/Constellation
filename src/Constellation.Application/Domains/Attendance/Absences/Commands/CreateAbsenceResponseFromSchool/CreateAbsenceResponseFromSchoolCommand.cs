namespace Constellation.Application.Domains.Attendance.Absences.Commands.CreateAbsenceResponseFromSchool;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record CreateAbsenceResponseFromSchoolCommand(
    AbsenceId AbsenceId,
    string Comment,
    string UserEmail)
    : ICommand;

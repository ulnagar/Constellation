namespace Constellation.Application.Absences.GetAbsenceDetailsForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetAbsenceDetailsForStudentQuery(
    AbsenceId AbsenceId)
    : IQuery<AbsenceForStudentResponse>;
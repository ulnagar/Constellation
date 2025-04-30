namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceDetailsForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetAbsenceDetailsForStudentQuery(
    AbsenceId AbsenceId)
    : IQuery<AbsenceForStudentResponse>;
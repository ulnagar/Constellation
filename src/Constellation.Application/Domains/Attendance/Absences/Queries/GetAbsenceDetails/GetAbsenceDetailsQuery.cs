namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Absences.Identifiers;

public sealed record GetAbsenceDetailsQuery(
    AbsenceId AbsenceId)
    : IQuery<AbsenceDetailsResponse>;
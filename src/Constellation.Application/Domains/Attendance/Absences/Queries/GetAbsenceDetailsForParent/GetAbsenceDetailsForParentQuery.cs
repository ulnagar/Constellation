namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceDetailsForParent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Absences.Identifiers;

public sealed record GetAbsenceDetailsForParentQuery(
    string ParentEmail,
    AbsenceId AbsenceId)
    : IQuery<ParentAbsenceDetailsResponse>;

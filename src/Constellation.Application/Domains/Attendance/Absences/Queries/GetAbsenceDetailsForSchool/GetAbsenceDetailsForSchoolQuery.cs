namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceDetailsForSchool;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Absences.Identifiers;

public sealed record GetAbsenceDetailsForSchoolQuery(
    AbsenceId AbsenceId)
    : IQuery<SchoolAbsenceDetailsResponse>;
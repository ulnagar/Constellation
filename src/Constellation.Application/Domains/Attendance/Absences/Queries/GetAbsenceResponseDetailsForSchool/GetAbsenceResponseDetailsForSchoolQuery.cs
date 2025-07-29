namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceResponseDetailsForSchool;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Absences.Identifiers;

public sealed record GetAbsenceResponseDetailsForSchoolQuery(
    AbsenceId AbsenceId,
    AbsenceResponseId ResponseId)
    : IQuery<SchoolAbsenceResponseDetailsResponse>;
namespace Constellation.Application.Absences.GetAbsenceResponseDetailsForSchool;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetAbsenceResponseDetailsForSchoolQuery(
    AbsenceId AbsenceId,
    AbsenceResponseId ResponseId)
    : IQuery<SchoolAbsenceResponseDetailsResponse>;
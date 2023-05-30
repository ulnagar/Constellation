namespace Constellation.Application.Absences.GetAbsenceDetailsForSchool;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetAbsenceDetailsForSchoolQuery(
    AbsenceId AbsenceId)
    : IQuery<SchoolAbsenceDetailsResponse>;
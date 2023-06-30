namespace Constellation.Application.Absences.GetAbsenceDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetAbsenceDetailsQuery(
    AbsenceId AbsenceId)
    : IQuery<AbsenceDetailsResponse>;
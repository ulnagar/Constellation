namespace Constellation.Application.Absences.GetAbsenceForStudentResponse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetAbsenceForStudentResponseQuery(
    AbsenceId AbsenceId)
    : IQuery<AbsenceForStudentResponse>;
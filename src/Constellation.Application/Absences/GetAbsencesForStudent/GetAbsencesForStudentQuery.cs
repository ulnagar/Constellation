namespace Constellation.Application.Absences.GetAbsencesForStudent;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAbsencesForStudentQuery(
    string StudentId)
    : IQuery<List<AbsenceForStudentResponse>>;
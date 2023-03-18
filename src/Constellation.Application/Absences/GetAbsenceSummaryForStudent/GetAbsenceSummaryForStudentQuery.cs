namespace Constellation.Application.Absences.GetAbsenceSummaryForStudent;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAbsenceSummaryForStudentQuery(
    string StudentId)
    : IQuery<List<StudentAbsenceSummaryResponse>>;

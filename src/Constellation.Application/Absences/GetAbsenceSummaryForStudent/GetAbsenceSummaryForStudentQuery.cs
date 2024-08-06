namespace Constellation.Application.Absences.GetAbsenceSummaryForStudent;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAbsenceSummaryForStudentQuery(
    string StudentId,
    bool OutstandingOnly = false)
    : IQuery<List<StudentAbsenceSummaryResponse>>;

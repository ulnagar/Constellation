namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsenceSummaryForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetAbsenceSummaryForStudentQuery(
    StudentId StudentId,
    bool OutstandingOnly = false)
    : IQuery<List<StudentAbsenceSummaryResponse>>;

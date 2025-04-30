namespace Constellation.Application.Domains.Attendance.Absences.Queries.GetAbsencesForExport;

using Abstractions.Messaging;
using DTOs;
using System.Collections.Generic;

public sealed record GetAbsencesForExportQuery(
    AbsenceFilterDto Filter)
    : IQuery<List<AbsenceExportResponse>>;